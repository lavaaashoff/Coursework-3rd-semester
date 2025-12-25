using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CouseWork3Semester.Interfaces;

namespace CouseWork3Semester.Services
{
    public class SettlementEvictionService : ISettlementEvictionService
    {
        public List<ISettlement> Settlements { get; private set; }
        public List<IEviction> Evictions { get; private set; }

        public SettlementEvictionService()
        {
            Settlements = new List<ISettlement>();
            Evictions = new List<IEviction>();
        }

        public SettlementEvictionService(List<ISettlement> initialSettlements, List<IEviction> initialEvictions)
        {
            Settlements = initialSettlements ?? new List<ISettlement>();
            Evictions = initialEvictions ?? new List<IEviction>();
        }

        public void AddSettlement(ISettlement settlement)
        {
            if (settlement == null)
                throw new ArgumentNullException(nameof(settlement), "Заселение не может быть null");

            if (Settlements.Any(s => s.Id == settlement.Id))
                throw new InvalidOperationException("Заселение с таким ID уже добавлено в сервис");

            Settlements.Add(settlement);
            Console.WriteLine($"Заселение добавлено: комната {settlement.Room?.Number}, {settlement.Occupants?.Count ?? 0} жильцов");
        }

        public void AddEviction(IEviction eviction)
        {
            if (eviction == null)
                throw new ArgumentNullException(nameof(eviction), "Выселение не может быть null");

            if (Evictions.Any(e => e.Id == eviction.Id))
                throw new InvalidOperationException("Выселение с таким ID уже добавлено в сервис");

            Evictions.Add(eviction);
            Console.WriteLine($"Выселение добавлено: комната {eviction.Room?.Number}, {eviction.Occupants?.Count ?? 0} жильцов");
        }

        public bool RemoveSettlement(Guid settlementId)
        {
            var s = Settlements.FirstOrDefault(x => x.Id == settlementId);
            if (s == null) return false;
            return Settlements.Remove(s);
        }

        public bool RemoveEviction(Guid evictionId)
        {
            var e = Evictions.FirstOrDefault(x => x.Id == evictionId);
            if (e == null) return false;
            return Evictions.Remove(e);
        }

        public ISettlement FindSettlementById(Guid id) => Settlements.FirstOrDefault(s => s.Id == id);
        public IEviction FindEvictionById(Guid id) => Evictions.FirstOrDefault(e => e.Id == id);

        public List<ISettlement> GetSettlementsForRoom(IRoom room)
        {
            if (room == null) return new List<ISettlement>();
            return Settlements.Where(s => s.Room?.Number == room.Number).ToList();
        }

        public List<IEviction> GetEvictionsForRoom(IRoom room)
        {
            if (room == null) return new List<IEviction>();
            return Evictions.Where(e => e.Room?.Number == room.Number).ToList();
        }

        public List<IRoomOccupant> GetOccupantsInRoom(IRoom room)
        {
            return room?.GetAllOccupants() ?? new List<IRoomOccupant>();
        }

        public bool IsRoomOccupied(IRoom room)
        {
            return GetRoomOccupancyCount(room) > 0;
        }

        public int GetRoomOccupancyCount(IRoom room)
        {
            return room?.GetAllOccupants()?.Count ?? 0;
        }

        public (int TotalSettlements, int TotalEvictions) GetStatistics()
        {
            return (Settlements.Count, Evictions.Count);
        }

        public string GetServiceInfo()
        {
            var stats = GetStatistics();
            var info = new StringBuilder();

            info.AppendLine("=== СЕРВИС ЗАСЕЛЕНИЙ И ВЫСЕЛЕНИЙ ===");
            info.AppendLine($"Всего заселений: {stats.TotalSettlements}");
            info.AppendLine($"Всего выселений: {stats.TotalEvictions}");

            var rooms = Settlements.Select(s => s.Room)
                .Concat(Evictions.Select(e => e.Room))
                .Where(r => r != null)
                .Distinct()
                .ToList();

            info.AppendLine($"\nКомнаты в обслуживании: {rooms.Count}");

            foreach (var room in rooms.Take(10))
            {
                var roomSettlements = GetSettlementsForRoom(room).Count;
                var roomEvictions = GetEvictionsForRoom(room).Count;
                var occupantsCount = GetRoomOccupancyCount(room);

                info.AppendLine($"  Комната {room.Number}: заселений={roomSettlements}, выселений={roomEvictions}, жильцов={occupantsCount}");
            }

            if (rooms.Count > 10)
            {
                info.AppendLine($"  ... и еще {rooms.Count - 10} комнат");
            }

            info.AppendLine("===================================");
            return info.ToString();
        }
    }
}