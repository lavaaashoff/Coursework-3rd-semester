using CouseWork3Semester.Interfaces;
using CouseWork3Semester.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Models
{
    public class Eviction : IEviction
    {
        // Константы для валидации
        private const int MAX_OCCUPANTS_PER_EVICTION = 10;

        // Свойства из UML (через интерфейсы)
        public DateTime EvictionDate { get; private set; } // ДатаВыселения
        public string Reason { get; private set; } // Причина
        public IRoom Room { get; private set; } // Комната
        public List<IRoomOccupant> Occupants { get; private set; } // Жильцы
        public ISettlement RelatedSettlement { get; private set; } // СвязанноеЗаселение

        // Дополнительные свойства
        private bool _isExecuted;
        private DateTime? _executionDate;

        // Статус выселения
        public EvictionStatus Status { get; private set; }

        // Конструктор
        public Eviction()
        {
            Occupants = new List<IRoomOccupant>();
            Status = EvictionStatus.Initialized;
            _isExecuted = false;
        }

        // Метод из UML: ИнициализироватьВыселение
        public void InitializeEviction(List<IRoomOccupant> occupants, IRoom room, string reason, ISettlement relatedSettlement)
        {
            ValidateInitialization(occupants, room, reason, relatedSettlement);

            Occupants = new List<IRoomOccupant>(occupants);
            Room = room;
            Reason = reason;
            RelatedSettlement = relatedSettlement;
            EvictionDate = DateTime.Now.Date;
            Status = EvictionStatus.Initialized;

            Console.WriteLine($"Выселение инициализировано: {occupants.Count} жильцов, комната {room.Number}, причина: {reason}");
        }

        // Метод из UML: ВыполнитьВыселение
        public bool PerformEviction()
        {
            ValidateBeforeEviction();

            try
            {
                Status = EvictionStatus.InProgress;

                // Проверяем, что все жильцы действительно находятся в этой комнате
                var currentRoomOccupants = Room.GetAllOccupants();
                var missingOccupants = new List<IRoomOccupant>();

                foreach (var occupant in Occupants)
                {
                    if (!currentRoomOccupants.Any(o => o.Id == occupant.Id))
                    {
                        missingOccupants.Add(occupant);
                    }
                }

                if (missingOccupants.Any())
                {
                    var names = string.Join(", ", missingOccupants.Select(o => o.FullName));
                    Console.WriteLine($"Предупреждение: следующие жильцы не находятся в комнате {Room.Number}: {names}");
                }

                // Удаляем жильцов из комнаты
                int successfullyEvicted = 0;
                foreach (var occupant in Occupants)
                {
                    bool removed = Room.RemoveOccupant(occupant.Id);
                    if (removed)
                    {
                        successfullyEvicted++;
                        Console.WriteLine($"Жилец {occupant.FullName} выселен из комнаты {Room.Number}");
                    }
                    else
                    {
                        Console.WriteLine($"Жилец {occupant.FullName} не найден в комнате {Room.Number}");
                    }
                }

                // Обновляем статус
                Status = EvictionStatus.Executed;
                _isExecuted = true;
                _executionDate = DateTime.Now;

                Console.WriteLine($"Выселение выполнено: {successfullyEvicted}/{Occupants.Count} жильцов выселено из комнаты {Room.Number}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при выполнении выселения: {ex.Message}");
                Status = EvictionStatus.Cancelled;
                return false;
            }
        }

        // Метод из UML: ПолучитьИнформациюОВыселении
        public string GetEvictionInfo()
        {
            var info = new StringBuilder();

            info.AppendLine("=== ИНФОРМАЦИЯ О ВЫСЕЛЕНИИ ===");
            info.AppendLine($"Дата выселения: {EvictionDate:dd.MM.yyyy}");
            info.AppendLine($"Комната: №{Room?.Number ?? 0} (Этаж: {Room?.Floor ?? 0})");
            info.AppendLine($"Причина: {Reason ?? "Не указана"}");
            info.AppendLine($"Статус: {GetStatusDescription()}");
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
                info.AppendLine($"Дата выполнения: {_executionDate.Value:dd.MM.yyyy HH:mm}");
            }


            info.AppendLine($"Текущих жильцов в комнате: {Room.GetAllOccupants().Count}");

            info.AppendLine("==============================");

            return info.ToString();
        }

        // Вспомогательные методы

        private void ValidateInitialization(List<IRoomOccupant> occupants, IRoom room, string reason, ISettlement relatedSettlement)
        {
            if (occupants == null || !occupants.Any())
            {
                throw new ArgumentException("Список жильцов не может быть пустым", nameof(occupants));
            }

            if (occupants.Count > MAX_OCCUPANTS_PER_EVICTION)
            {
                throw new ArgumentException($"Максимальное количество жильцов для выселения: {MAX_OCCUPANTS_PER_EVICTION}", nameof(occupants));
            }

            if (room == null)
            {
                throw new ArgumentNullException(nameof(room), "Комната не может быть null");
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                throw new ArgumentException("Причина выселения не может быть пустой", nameof(reason));
            }

            if (relatedSettlement == null)
            {
                throw new ArgumentNullException(nameof(relatedSettlement), "Связанное заселение не может быть null");
            }

            // Проверяем, что заселение было завершено
            if (!relatedSettlement.IsActive && relatedSettlement is Settlement settlement)
            {
                // Можно добавить дополнительную проверку статуса заселения
            }
        }

        private void ValidateBeforeEviction()
        {
            if (Room == null)
            {
                throw new InvalidOperationException("Комната не указана");
            }

            if (!Occupants.Any())
            {
                throw new InvalidOperationException("Нет жильцов для выселения");
            }

            if (string.IsNullOrWhiteSpace(Reason))
            {
                throw new InvalidOperationException("Причина выселения не указана");
            }

            if (RelatedSettlement == null)
            {
                throw new InvalidOperationException("Связанное заселение не указано");
            }

            if (Status == EvictionStatus.Executed)
            {
                throw new InvalidOperationException("Выселение уже выполнено");
            }

            if (Status == EvictionStatus.Cancelled)
            {
                throw new InvalidOperationException("Выселение было отменено");
            }
        }

        private string GetStatusDescription()
        {
            return Status switch
            {
                EvictionStatus.Initialized => "Инициализировано",
                EvictionStatus.InProgress => "В процессе",
                EvictionStatus.Executed => "Выполнено",
                EvictionStatus.Cancelled => "Отменено",
                _ => "Неизвестно"
            };
        }

        // Дополнительные методы для удобства

        public bool CanEvictOccupant(IRoomOccupant occupant)
        {
            if (occupant == null)
                return false;

            // Проверяем, не добавлен ли уже этот жилец
            if (Occupants.Any(o => o.Id == occupant.Id))
                return false;

            // Проверяем, что жилец действительно находится в комнате
            if (Room != null && !IsOccupantInRoom(occupant.Id))
                return false;

            return true;
        }

        public void AddOccupantToEviction(IRoomOccupant occupant)
        {
            if (!CanEvictOccupant(occupant))
            {
                throw new InvalidOperationException($"Нельзя добавить жильца {occupant.FullName} к выселению");
            }

            Occupants.Add(occupant);
            Console.WriteLine($"Жилец {occupant.FullName} добавлен к выселению");
        }

        public bool RemoveOccupantFromEviction(Guid occupantId)
        {
            var occupant = Occupants.FirstOrDefault(o => o.Id == occupantId);
            if (occupant == null)
                return false;

            Occupants.Remove(occupant);
            Console.WriteLine($"Жилец {occupant.FullName} удален из списка выселения");
            return true;
        }

        public bool IsOccupantInRoom(Guid occupantId)
        {
            if (Room == null)
                return false;

            var currentOccupants = Room.GetAllOccupants();
            return currentOccupants.Any(o => o.Id == occupantId);
        }



        // Отменить выселение
        public void CancelEviction(string cancellationReason)
        {
            if (Status == EvictionStatus.Executed)
            {
                throw new InvalidOperationException("Нельзя отменить уже выполненное выселение");
            }

            Status = EvictionStatus.Cancelled;
            Reason += $"\nОтменено: {cancellationReason}";

            Console.WriteLine($"Выселение отменено. Дополнительная причина: {cancellationReason}");
        }

        // Проверить, можно ли выселить всех жильцов из комнаты
        public bool CanEvictAllFromRoom()
        {
            if (Room == null)
                return false;

            var currentOccupants = Room.GetAllOccupants();
            return Occupants.All(o => currentOccupants.Any(co => co.Id == o.Id));
        }

        // Получить список жильцов, которые не находятся в комнате
        public List<IRoomOccupant> GetMissingOccupants()
        {
            if (Room == null)
                return new List<IRoomOccupant>(Occupants);

            var currentOccupants = Room.GetAllOccupants();
            return Occupants.Where(o => !currentOccupants.Any(co => co.Id == o.Id)).ToList();
        }
    }
}
