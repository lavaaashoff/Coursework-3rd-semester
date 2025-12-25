using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CouseWork3Semester.Interfaces;

namespace CouseWork3Semester.Models
{
    public class Eviction : IEviction
    {
        private const int MAX_OCCUPANTS_PER_EVICTION = 10;

        public Guid Id { get; private set; }
        public DateTime EvictionDate { get; private set; }
        public string Reason { get; private set; }
        public IRoom Room { get; private set; }
        public List<IRoomOccupant> Occupants { get; private set; }
        public ISettlement RelatedSettlement { get; private set; }

        private DateTime? _executionDate;

        public Eviction()
        {
            Id = Guid.NewGuid();
            Occupants = new List<IRoomOccupant>();
        }

        public void InitializeEviction(Guid ID, List<IRoomOccupant> occupants, IRoom room, string reason, ISettlement relatedSettlement)
        {
            if (occupants == null || !occupants.Any())
                throw new ArgumentException("Список жильцов не может быть пустым", nameof(occupants));
            if (occupants.Count > MAX_OCCUPANTS_PER_EVICTION)
                throw new ArgumentException($"Максимальное количество жильцов для выселения: {MAX_OCCUPANTS_PER_EVICTION}", nameof(occupants));
            if (room == null)
                throw new ArgumentNullException(nameof(room), "Комната не может быть null");
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Причина выселения не может быть пустой", nameof(reason));
            if (relatedSettlement == null)
                throw new ArgumentNullException(nameof(relatedSettlement), "Связанное заселение не может быть null");

            Id = ID;
            Occupants = new List<IRoomOccupant>(occupants);
            Room = room;
            Reason = reason;
            RelatedSettlement = relatedSettlement;
            EvictionDate = DateTime.Now.Date;

            var currentRoomOccupants = Room.GetAllOccupants() ?? new List<IRoomOccupant>();
            var missing = Occupants.Where(o => !currentRoomOccupants.Any(co => co.Id == o.Id)).ToList();
            if (missing.Any())
            {
                var names = string.Join(", ", missing.Select(o => o.FullName));
                Console.WriteLine($"Предупреждение: жильцы не найдены в комнате {Room.Number}: {names}");
            }

            int evicted = 0;
            foreach (var occ in Occupants)
            {
                if (Room.RemoveOccupant(occ.Id))
                {
                    evicted++;
                    Console.WriteLine($"Жилец {occ.FullName} выселен из комнаты {Room.Number}");
                }
            }

            _executionDate = DateTime.Now;
            Console.WriteLine($"Выселение завершено: {evicted}/{Occupants.Count} жильцов выселены ({EvictionDate:dd.MM.yyyy})");
        }

        public bool CanEvictOccupant(IRoomOccupant occupant)
        {
            if (Room == null || occupant == null) return false;
            return IsOccupantInRoom(occupant.Id);
        }

        public void AddOccupantToEviction(IRoomOccupant occupant)
        {
            if (occupant == null) return;
            if (!Occupants.Any(o => o.Id == occupant.Id))
                Occupants.Add(occupant);
        }

        public bool RemoveOccupantFromEviction(Guid occupantId)
        {
            var found = Occupants.FirstOrDefault(o => o.Id == occupantId);
            if (found == null) return false;
            return Occupants.Remove(found);
        }

        public bool IsOccupantInRoom(Guid occupantId)
        {
            return Room?.GetAllOccupants()?.Any(o => o.Id == occupantId) ?? false;
        }

        public string GetEvictionInfo()
        {
            var info = new StringBuilder();

            info.AppendLine("=== ИНФОРМАЦИЯ О ВЫСЕЛЕНИИ ===");
            info.AppendLine($"Дата выселения: {EvictionDate:dd.MM.yyyy}");
            info.AppendLine($"Комната: №{Room?.Number ?? 0} (Этаж: {Room?.Floor ?? 0})");
            info.AppendLine($"Причина: {Reason ?? "Не указана"}");
            info.AppendLine($"Количество жильцов для выселения: {Occupants.Count}");

            if (Occupants.Any())
            {
                info.AppendLine("Жильцы для выселения:");
                foreach (var occupant in Occupants)
                {
                    info.AppendLine($"  - {occupant.FullName} (возраст: {occupant.GetAge()})");
                }
            }

            if (RelatedSettlement != null)
            {
                info.AppendLine($"Связанное заселение: {RelatedSettlement.SettlementDate:dd.MM.yyyy}");
            }

            if (_executionDate.HasValue)
            {
                info.AppendLine($"Дата фиксации выполнения: {_executionDate.Value:dd.MM.yyyy HH:mm}");
            }

            info.AppendLine($"Текущих жильцов в комнате: {Room?.GetAllOccupants().Count ?? 0}");
            info.AppendLine("==============================");

            return info.ToString();
        }
    }
}