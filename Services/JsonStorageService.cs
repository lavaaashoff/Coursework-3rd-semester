using System;
using System.IO;
using System.Linq;
using System.Text;
using CouseWork3Semester.Interfaces;
using CouseWork3Semester.Models;
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
                InventoryRegistry = system.InventoryRegistry as Registries.InventoryRegistry ?? new Registries.InventoryRegistry(),
                DocumentLinks = (system.DocumentOccupantService?.GetAllLinks() ?? new System.Collections.Generic.List<IDocumentOccupantLink>())
                                .Select(l => l as DocumentOccupantLink ?? new DocumentOccupantLink(l.Id, l.DocumentId, l.OccupantId, DateTime.Now))
                                .ToList()
            };

            var dir = Path.GetDirectoryName(StoragePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

            var json = JsonConvert.SerializeObject(state, _settings);

            var tmpPath = StoragePath + ".tmp";
            var bakPath = StoragePath + ".bak";

            using (var fs = new FileStream(tmpPath, FileMode.Create, FileAccess.Write, FileShare.None, 64 * 1024, FileOptions.None))
            using (var writer = new StreamWriter(fs, new UTF8Encoding(false)))
            {
                writer.Write(json);
                writer.Flush();
                fs.Flush(true);
            }

            try { if (File.Exists(StoragePath)) File.Copy(StoragePath, bakPath, overwrite: true); } catch { }
            try { File.Copy(tmpPath, StoragePath, overwrite: true); } finally { try { File.Delete(tmpPath); } catch { } }
        }

        public bool TryLoad(out DataState state)
        {
            state = null;
            var path = StoragePath;
            if (!File.Exists(path))
            {
                var bak = StoragePath + ".bak";
                if (!File.Exists(bak)) return false;
                path = bak;
            }

            try
            {
                var info = new FileInfo(path);
                if (!info.Exists || info.Length < 2) return false;

                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = new StreamReader(fs, Encoding.UTF8, true))
                {
                    var json = reader.ReadToEnd();
                    state = JsonConvert.DeserializeObject<DataState>(json, _settings);
                    return state != null;
                }
            }
            catch
            {
                state = null;
                return false;
            }
        }
    }
}