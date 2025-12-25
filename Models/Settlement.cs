using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CouseWork3Semester.Interfaces;

namespace CouseWork3Semester.Models
{
    public class Settlement : ISettlement
    {
        // Константы для валидации
        private const int MAX_OCCUPANTS_PER_SETTLEMENT = 10;
        private const int MIN_OCCUPANTS_PER_SETTLEMENT = 1;

        // Свойства (без статусов и активности)
        public Guid Id { get; private set; }
        public DateTime SettlementDate { get; private set; }
        public List<IRoomOccupant> Occupants { get; private set; }
        public IRoom Room { get; private set; }
        public IDocument Document { get; private set; }

        // Дополнительно — дата завершения операции (для информирования)
        private DateTime? _completionDate;

        public Settlement()
        {
            Id = Guid.NewGuid();
            Occupants = new List<IRoomOccupant>();
        }

        // Упрощённая логика: инициализация сразу выполняет заселение (без статусов)
        public void InitializeSettlement(List<IRoomOccupant> occupants, IRoom room, IDocument document, DateTime date)
        {
            if (occupants == null || occupants.Count < MIN_OCCUPANTS_PER_SETTLEMENT)
                throw new ArgumentException($"Список жильцов не может быть пустым. Минимум: {MIN_OCCUPANTS_PER_SETTLEMENT}", nameof(occupants));
            if (occupants.Count > MAX_OCCUPANTS_PER_SETTLEMENT)
                throw new ArgumentException($"Максимум жильцов для одного заселения: {MAX_OCCUPANTS_PER_SETTLEMENT}", nameof(occupants));
            if (room == null)
                throw new ArgumentNullException(nameof(room), "Комната не может быть null");

            // Проверка вместимости комнаты
            var current = room.GetAllOccupants()?.Count ?? 0;
            var capacity = room.Type;
            if (current + occupants.Count > capacity)
                throw new InvalidOperationException($"Недостаточно мест в комнате {room.Number}. Доступно: {capacity - current}");

            SettlementDate = date.Date;
            Room = room;
            Document = document;
            Occupants = new List<IRoomOccupant>(occupants);

            // Сразу заселяем
            var added = new List<IRoomOccupant>();
            foreach (var occ in Occupants)
            {
                bool ok = Room.AddOccupant(occ);
                if (!ok)
                {
                    // Откат добавленных
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
                foreach (var occupant in Occupants)
                {
                    info.AppendLine($"  - {occupant.FullName} (возраст: {occupant.GetAge()}, тип: {occupant.GetOccupantType()})");
                }
            }

            info.AppendLine($"Свободных мест в комнате: {Room?.GetAvailablePlacesCount() ?? 0}");

            if (_completionDate.HasValue)
            {
                info.AppendLine($"Дата фиксации выполнения: {_completionDate.Value:dd.MM.yyyy HH:mm}");
            }

            info.AppendLine("==============================");
            return info.ToString();
        }
    }
}