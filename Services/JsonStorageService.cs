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
            StoragePath = string.IsNullOrWhiteSpace(storagePath)
                ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Storage", "state.json")
                : storagePath;

            _settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                PreserveReferencesHandling = PreserveReferencesHandling.All,
                Formatting = Formatting.Indented
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


            using var fs = new FileStream(
                StoragePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 64 * 1024,
                options: FileOptions.WriteThrough 
            );
            using var writer = new StreamWriter(fs, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            writer.Write(json);
            writer.Flush();
            fs.Flush(true);
        }

        public bool TryLoad(out DataState state)
        {
            state = null;

            if (!File.Exists(StoragePath))
                return false;

            using var fs = new FileStream(
                StoragePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read
            );
            using var reader = new StreamReader(fs, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            var json = reader.ReadToEnd();

            state = JsonConvert.DeserializeObject<DataState>(json, _settings);
            return state != null;
        }
    }
}