using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CouseWork3Semester.Interfaces;
using Newtonsoft.Json;

namespace CouseWork3Semester.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Settlement : ISettlement
    {
        private const int MAX_OCCUPANTS_PER_SETTLEMENT = 10;
        private const int MIN_OCCUPANTS_PER_SETTLEMENT = 1;

        [JsonProperty] public Guid Id { get; private set; }
        [JsonProperty] public DateTime SettlementDate { get; private set; }
        [JsonProperty] public List<IRoomOccupant> Occupants { get; private set; }
        [JsonProperty] public IRoom Room { get; private set; }
        [JsonProperty] public IDocument Document { get; private set; }

        private DateTime? _completionDate;

        public Settlement()
        {
            Id = Guid.NewGuid();
            Occupants = new List<IRoomOccupant>();
        }

        public void InitializeSettlement(List<IRoomOccupant> occupants, IRoom room, IDocument document, DateTime date)
        {
            if (occupants == null || occupants.Count < MIN_OCCUPANTS_PER_SETTLEMENT)
                throw new ArgumentException($"Список жильцов не может быть пустым. Минимум: {MIN_OCCUPANTS_PER_SETTLEMENT}", nameof(occupants));
            if (occupants.Count > MAX_OCCUPANTS_PER_SETTLEMENT)
                throw new ArgumentException($"Максимум жильцов для одного заселения: {MAX_OCCUPANTS_PER_SETTLEMENT}", nameof(occupants));
            if (room == null)
                throw new ArgumentNullException(nameof(room), "Комната не может быть null");

            var current = room.GetAllOccupants()?.Count ?? 0;
            var capacity = room.Type;
            if (current + occupants.Count > capacity)
                throw new InvalidOperationException($"Недостаточно мест в комнате {room.Number}. Доступно: {capacity - current}");

            SettlementDate = date.Date;
            Room = room;
            Document = document;
            Occupants = new List<IRoomOccupant>(occupants);

            var added = new List<IRoomOccupant>();
            foreach (var occ in Occupants)
            {
                bool ok = Room.AddOccupant(occ);
                if (!ok)
                {
                    foreach (var a in added)
                        Room.RemoveOccupant(a.Id);
                    throw new InvalidOperationException($"Не удалось заселить жильца {occ.FullName} в комнату {Room.Number}");
                }
                added.Add(occ);
            }

            _completionDate = DateTime.Now;
            Console.WriteLine($"Заселение завершено: {Occupants.Count} жильцов заселены в комнату {Room.Number} ({SettlementDate:dd.MM.yyyy})");
        }

        public string GetSettlementInfo()
        {
            var info = new StringBuilder();

            info.AppendLine("=== ИНФОРМАЦИЯ О ЗАСЕЛЕНИИ ===");
            info.AppendLine($"Дата заселения: {SettlementDate:dd.MM.yyyy}");
            info.AppendLine($"Комната: №{Room?.Number ?? 0} (Этаж: {Room?.Floor ?? 0}, Площадь: {Room?.Area ?? 0} м²)");
            info.AppendLine($"Документ: {Document?.Title ?? "Не указан"}");
            info.AppendLine($"Количество жильцов: {Occupants.Count}");

            if (Occupants.Any())
            {
                info.AppendLine("Жильцы:");
                foreach (var o in Occupants)
                {
                    info.AppendLine($"- {o.FullName} ({o.GetOccupantType()}), {o.BirthDate:dd.MM.yyyy}");
                }
            }

            return info.ToString();
        }
    }
}