using System;
using System.IO;
using System.Text;
using CouseWork3Semester.Interfaces;
using Newtonsoft.Json;

namespace CouseWork3Semester.Services
{
    public class JsonStorageService : IStorageService
    {
        private readonly JsonSerializerSettings _settings;

        public string StoragePath { get; }

        public JsonStorageService(string storagePath)
        {
            // Только папка проекта: ./Storage/state.json
            StoragePath = string.IsNullOrWhiteSpace(storagePath)
                ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Storage", "state.json")
                : storagePath;

            _settings = new JsonSerializerSettings
            {
                // Полные метаданные типов — надёжно для интерфейсов
                TypeNameHandling = TypeNameHandling.All,
                PreserveReferencesHandling = PreserveReferencesHandling.All,
                Formatting = Formatting.Indented,
                MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
            };
        }

        public void Save(IAccountingSystem system)
        {
            if (system == null) throw new ArgumentNullException(nameof(system));

            var state = new DataState
            {
                DormitoryRegistry = system.DormitoryRegistry as Registries.DormitoryRegistry ?? new Registries.DormitoryRegistry(),
                OccupantRegistry = system.OccupantRegistry as Registries.OccupantRegistry ?? new Registries.OccupantRegistry(),
                DocumentRegistry = system.DocumentRegistry as Registries.DocumentRegistry ?? new Registries.DocumentRegistry(),
                SettlementEvictionService = system.SettlementEvictionService as SettlementEvictionService ?? new SettlementEvictionService(),
                InventoryRegistry = system.InventoryRegistry as Registries.InventoryRegistry ?? new Registries.InventoryRegistry()
            };

            var dir = Path.GetDirectoryName(StoragePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var json = JsonConvert.SerializeObject(state, _settings);

            // Атомарная запись: во временный файл + замена, плюс .bak
            var tmpPath = StoragePath + ".tmp";
            var bakPath = StoragePath + ".bak";

            using (var fs = new FileStream(
                tmpPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 64 * 1024,
                FileOptions.None))
            using (var writer = new StreamWriter(fs, new UTF8Encoding(false)))
            {
                writer.Write(json);
                writer.Flush();
                fs.Flush(true);
            }

            try
            {
                if (File.Exists(StoragePath))
                {
                    File.Copy(StoragePath, bakPath, overwrite: true);
                }
            }
            catch { /* резервная копия необязательна */ }

            try
            {
                File.Copy(tmpPath, StoragePath, overwrite: true);
            }
            finally
            {
                try { if (File.Exists(tmpPath)) File.Delete(tmpPath); } catch { }
            }
        }

        public bool TryLoad(out DataState state)
        {
            state = null;

            // Читаем только из ./Storage/state.json; без AppData
            var pathToRead = StoragePath;
            if (!File.Exists(pathToRead))
            {
                // Если основного нет — пробуем резервную копию
                var bakPath = StoragePath + ".bak";
                if (!File.Exists(bakPath)) return false;
                pathToRead = bakPath;
            }

            try
            {
                var info = new FileInfo(pathToRead);
                if (!info.Exists || info.Length < 2)
                    return false;

                using (var fs = new FileStream(
                    pathToRead,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite))
                using (var reader = new StreamReader(fs, Encoding.UTF8, true))
                {
                    var json = reader.ReadToEnd();
                    state = JsonConvert.DeserializeObject<DataState>(json, _settings);
                    return state != null;
                }
            }
            catch
            {
                // Падение на основном/резервном — считаем, что состояния нет
                state = null;
                return false;
            }
        }
    }
}