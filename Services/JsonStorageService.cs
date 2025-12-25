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
                TypeNameHandling = TypeNameHandling.All,
                PreserveReferencesHandling = PreserveReferencesHandling.All,
                Formatting = Formatting.Indented,
                MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
            };

            DebugLogger.Write($"JsonStorageService initialized. StoragePath={StoragePath}");
        }

        public void Save(IAccountingSystem system)
        {
            if (system == null) throw new ArgumentNullException(nameof(system));
            DebugLogger.Write("Save() started");

            var state = new DataState
            {
                DormitoryRegistry = system.DormitoryRegistry as Registries.DormitoryRegistry ?? new Registries.DormitoryRegistry(),
                OccupantRegistry = system.OccupantRegistry as Registries.OccupantRegistry ?? new Registries.OccupantRegistry(),
                DocumentRegistry = system.DocumentRegistry as Registries.DocumentRegistry ?? new Registries.DocumentRegistry(),
                SettlementEvictionService = system.SettlementEvictionService as SettlementEvictionService ?? new SettlementEvictionService(),
                InventoryRegistry = system.InventoryRegistry as Registries.InventoryRegistry ?? new Registries.InventoryRegistry()
            };

            try
            {
                var dir = Path.GetDirectoryName(StoragePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var json = JsonConvert.SerializeObject(state, _settings);
                DebugLogger.Write($"Save() JSON length={json?.Length ?? 0}");

                // Атомарная запись: tmp + .bak + замена
                var tmpPath = StoragePath + ".tmp";
                var bakPath = StoragePath + ".bak";

                using (var fs = new FileStream(tmpPath, FileMode.Create, FileAccess.Write, FileShare.None, 64 * 1024, FileOptions.None))
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
                        DebugLogger.Write("Save() backup created (.bak)");
                    }
                }
                catch (Exception exBak)
                {
                    DebugLogger.Write("Save() backup failed", exBak);
                }

                File.Copy(tmpPath, StoragePath, overwrite: true);
                DebugLogger.Write("Save() file replaced");

                try { if (File.Exists(tmpPath)) File.Delete(tmpPath); } catch { }
                DebugLogger.Write("Save() completed");
            }
            catch (Exception ex)
            {
                DebugLogger.Write("Save() failed", ex);
                throw;
            }
        }

        public bool TryLoad(out DataState state)
        {
            state = null;
            var pathToRead = StoragePath;
            DebugLogger.Write($"TryLoad() path={pathToRead}");

            if (!File.Exists(pathToRead))
            {
                var bak = StoragePath + ".bak";
                if (!File.Exists(bak))
                {
                    DebugLogger.Write("TryLoad() file not found");
                    return false;
                }
                pathToRead = bak;
                DebugLogger.Write("TryLoad() fallback to .bak");
            }

            try
            {
                var info = new FileInfo(pathToRead);
                DebugLogger.Write($"TryLoad() file exists. size={info.Length} bytes");

                if (!info.Exists || info.Length < 2)
                {
                    DebugLogger.Write("TryLoad() file too small");
                    return false;
                }

                using (var fs = new FileStream(pathToRead, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = new StreamReader(fs, Encoding.UTF8, true))
                {
                    var json = reader.ReadToEnd();
                    DebugLogger.Write($"TryLoad() read json length={json?.Length ?? 0}");

                    state = JsonConvert.DeserializeObject<DataState>(json, _settings);
                    if (state == null)
                    {
                        DebugLogger.Write("TryLoad() deserialized state is null");
                        return false;
                    }

                    // Быстрая сводка
                    try
                    {
                        var dorms = state.DormitoryRegistry?.Dormitories?.Count ?? -1;
                        var occs = state.OccupantRegistry?.AllOccupants?.Count ?? -1;
                        var docs = state.DocumentRegistry?.Documents?.Count ?? -1;
                        var sets = state.SettlementEvictionService?.Settlements?.Count ?? -1;
                        var evs = state.SettlementEvictionService?.Evictions?.Count ?? -1;
                        var items = state.InventoryRegistry?.GetAllItems()?.Count ?? -1;

                        DebugLogger.Write($"TryLoad() summary: Dormitories={dorms}, Occupants={occs}, Documents={docs}, Settlements={sets}, Evictions={evs}, InventoryItems={items}");
                    }
                    catch (Exception exSumm)
                    {
                        DebugLogger.Write("TryLoad() summary failed", exSumm);
                    }
                }

                DebugLogger.Write("TryLoad() success");
                return true;
            }
            catch (Exception ex)
            {
                DebugLogger.Write("TryLoad() failed", ex);
                state = null;
                return false;
            }
        }
    }
}