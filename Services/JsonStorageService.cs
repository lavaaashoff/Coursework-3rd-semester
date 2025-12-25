using System;
using System.IO;
using System.Text;
using CouseWork3Semester.Interfaces;
using CouseWork3Semester.Registries;
using Newtonsoft.Json;

namespace CouseWork3Semester.Services
{
    public class JsonStorageService : IStorageService
    {
        private readonly JsonSerializerSettings _settings;

        public string StoragePath { get; }

        public JsonStorageService(string storagePath)
        {
            StoragePath = storagePath;
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
                DormitoryRegistry = system.DormitoryRegistry as DormitoryRegistry ?? new DormitoryRegistry(),
                OccupantRegistry = system.OccupantRegistry as OccupantRegistry ?? new OccupantRegistry(),
                DocumentRegistry = system.DocumentRegistry as DocumentRegistry ?? new DocumentRegistry(),
                SettlementEvictionService = system.SettlementEvictionService as SettlementEvictionService ?? new SettlementEvictionService(),
                InventoryRegistry = system.InventoryRegistry as InventoryRegistry ?? new InventoryRegistry()
            };

            var dir = Path.GetDirectoryName(StoragePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var json = JsonConvert.SerializeObject(state, _settings);
            File.WriteAllText(StoragePath, json, Encoding.UTF8);
        }

        public bool TryLoad(out DataState state)
        {
            state = null;
            if (!File.Exists(StoragePath))
                return false;

            var json = File.ReadAllText(StoragePath, Encoding.UTF8);
            state = JsonConvert.DeserializeObject<DataState>(json, _settings);
            return state != null;
        }
    }
}