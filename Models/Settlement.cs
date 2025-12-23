using CouseWork3Semester.Interfaces;
using CouseWork3Semester.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Models
{
    public class Settlement : ISettlement
    {
        // Константы для валидации
        private const int MAX_OCCUPANTS_PER_SETTLEMENT = 10;
        private const int MIN_OCCUPANTS_PER_SETTLEMENT = 1;

        // Свойства из UML (через интерфейсы)
        public DateTime SettlementDate { get; private set; } // ДатаЗаселения
        public List<IRoomOccupant> Occupants { get; private set; } // Жильцы
        public IRoom Room { get; private set; } // Комната
        public IDocument Document { get; private set; } // Документ
        public bool IsActive { get; private set; } // Активно

        // Дополнительные свойства
        private DateTime? _completionDate;
        private string _cancellationReason;
        private bool _isCancelled;



        public SettlementStatus Status { get; private set; }

        // Конструктор
        public Settlement()
        {
            Occupants = new List<IRoomOccupant>();
            Status = SettlementStatus.Initialized;
            IsActive = false;
        }

        // Метод из UML: ИнициализироватьЗаселение
        public void InitializeSettlement(List<IRoomOccupant> occupants, IRoom room, IDocument document, DateTime date)
        {
            ValidateInitialization(occupants, room, document, date);

            Occupants = new List<IRoomOccupant>(occupants);
            Room = room;
            Document = document;
            SettlementDate = date.Date;
            Status = SettlementStatus.Initialized;
            IsActive = false;

            Console.WriteLine($"Заселение инициализировано: {occupants.Count} жильцов, комната {room.Number}, дата {date:dd.MM.yyyy}");
        }

        // Метод из UML: ВыполнитьЗаселение
        public void PerformSettlement()
        {
            ValidateBeforeSettlement();

            // Проверяем, что в комнате есть свободные места
            if (!Room.CheckAvailablePlaces())
            {
                throw new InvalidOperationException($"В комнате {Room.Number} нет свободных мест");
            }

            // Проверяем, что есть достаточно мест для всех жильцов
            int availablePlaces = Room.GetAvailablePlacesCount();
            if (availablePlaces < Occupants.Count)
            {
                throw new InvalidOperationException($"В комнате {Room.Number} только {availablePlaces} свободных мест, а нужно {Occupants.Count}");
            }

            // Добавляем каждого жильца в комнату
            foreach (var occupant in Occupants)
            {
                bool added = Room.AddOccupant(occupant);
                if (!added)
                {
                    // Откатываем добавление предыдущих жильцов
                    foreach (var addedOccupant in Occupants.TakeWhile(o => o != occupant))
                    {
                        Room.RemoveOccupant(addedOccupant.Id);
                    }
                    throw new InvalidOperationException($"Не удалось заселить жильца {occupant.FullName} в комнату {Room.Number}");
                }
            }

            Status = SettlementStatus.InProgress;
            IsActive = true;

            Console.WriteLine($"Заселение выполнено: {Occupants.Count} жильцов заселены в комнату {Room.Number}");
        }

        // Метод из UML: ОтменитьЗаселение
        public void CancelSettlement(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                throw new ArgumentException("Причина отмены не может быть пустой", nameof(reason));
            }

            if (Status == SettlementStatus.Completed)
            {
                throw new InvalidOperationException("Нельзя отменить уже завершенное заселение");
            }

            // Если заселение было в процессе, удаляем жильцов из комнаты
            if (Status == SettlementStatus.InProgress && Room != null)
            {
                foreach (var occupant in Occupants)
                {
                    Room.RemoveOccupant(occupant.Id);
                }
            }

            Status = SettlementStatus.Cancelled;
            IsActive = false;
            _cancellationReason = reason;
            _isCancelled = true;

            Console.WriteLine($"Заселение отменено. Причина: {reason}");
        }

        // Метод из UML: ЗавершитьЗаселение
        public void CompleteSettlement()
        {
            if (Status != SettlementStatus.InProgress)
            {
                throw new InvalidOperationException("Можно завершить только заселение в процессе");
            }

            Status = SettlementStatus.Completed;
            IsActive = false;
            _completionDate = DateTime.Now;

            // Жильцы остаются в комнате (заселение завершено успешно)
            Console.WriteLine($"Заселение завершено: жильцы проживают в комнате {Room.Number}");
        }

        // Метод из UML: ПолучитьИнформациюОЗаселении
        public string GetSettlementInfo()
        {
            var info = new StringBuilder();

            info.AppendLine("=== ИНФОРМАЦИЯ О ЗАСЕЛЕНИИ ===");
            info.AppendLine($"Дата заселения: {SettlementDate:dd.MM.yyyy}");
            info.AppendLine($"Комната: №{Room?.Number ?? 0} (Этаж: {Room?.Floor ?? 0}, Площадь: {Room?.Area ?? 0} м²)");
            info.AppendLine($"Документ: {Document?.Title ?? "Не указан"}");
            info.AppendLine($"Статус: {GetStatusDescription()}");
            info.AppendLine($"Активно: {(IsActive ? "Да" : "Нет")}");
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
                info.AppendLine($"Дата завершения: {_completionDate.Value:dd.MM.yyyy}");
            }

            if (_isCancelled)
            {
                info.AppendLine($"Причина отмены: {_cancellationReason}");
            }

            info.AppendLine("==============================");

            return info.ToString();
        }

        // Вспомогательные методы

        private void ValidateInitialization(List<IRoomOccupant> occupants, IRoom room, IDocument document, DateTime date)
        {
            if (occupants == null || !occupants.Any())
            {
                throw new ArgumentException("Список жильцов не может быть пустым", nameof(occupants));
            }

            if (occupants.Count > MAX_OCCUPANTS_PER_SETTLEMENT)
            {
                throw new ArgumentException($"Максимальное количество жильцов: {MAX_OCCUPANTS_PER_SETTLEMENT}", nameof(occupants));
            }

            if (room == null)
            {
                throw new ArgumentNullException(nameof(room), "Комната не может быть null");
            }

            if (document == null)
            {
                throw new ArgumentNullException(nameof(document), "Документ не может быть null");
            }

            if (date > DateTime.Now)
            {
                throw new ArgumentException("Дата заселения не может быть в будущем", nameof(date));
            }
        }

        private void ValidateBeforeSettlement()
        {
            if (Room == null)
            {
                throw new InvalidOperationException("Комната не указана");
            }

            if (!Occupants.Any())
            {
                throw new InvalidOperationException("Нет жильцов для заселения");
            }

            if (Document == null)
            {
                throw new InvalidOperationException("Документ не указан");
            }

            if (Status == SettlementStatus.Cancelled)
            {
                throw new InvalidOperationException("Заселение было отменено");
            }

            if (Status == SettlementStatus.Completed)
            {
                throw new InvalidOperationException("Заселение уже завершено");
            }
        }

        private string GetStatusDescription()
        {
            return Status switch
            {
                SettlementStatus.Initialized => "Инициализировано",
                SettlementStatus.InProgress => "В процессе",
                SettlementStatus.Completed => "Завершено",
                SettlementStatus.Cancelled => "Отменено",
                _ => "Неизвестно"
            };
        }

        // Дополнительные методы для удобства

        public bool CanAddOccupant(IRoomOccupant occupant)
        {
            if (occupant == null)
                return false;

            // Проверяем, не добавлен ли уже этот жилец
            if (Occupants.Any(o => o.Id == occupant.Id))
                return false;

            // Проверяем, есть ли место в комнате
            if (Room != null && Room.GetAvailablePlacesCount() < 1)
                return false;

            return true;
        }

        public void AddOccupant(IRoomOccupant occupant)
        {
            if (!CanAddOccupant(occupant))
            {
                throw new InvalidOperationException($"Нельзя добавить жильца {occupant.FullName}");
            }

            Occupants.Add(occupant);
            Console.WriteLine($"Жилец {occupant.FullName} добавлен к заселению");
        }

        public bool RemoveOccupant(Guid occupantId)
        {
            var occupant = Occupants.FirstOrDefault(o => o.Id == occupantId);
            if (occupant == null)
                return false;

            // Если заселение в процессе, удаляем жильца из комнаты
            if (Status == SettlementStatus.InProgress && Room != null)
            {
                Room.RemoveOccupant(occupantId);
            }

            Occupants.Remove(occupant);
            Console.WriteLine($"Жилец {occupant.FullName} удален из заселения");
            return true;
        }
    }
}
