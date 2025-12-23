using CouseWork3Semester.Interfaces;
using CouseWork3Semester.Models;
using CouseWork3Semester.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Services
{
    public class SettlementEvictionService : ISettlementEvictionService
    {
        // Свойства из UML (через интерфейсы)
        public List<ISettlement> Settlements { get; private set; } // Заселения
        public List<IEviction> Evictions { get; private set; } // Выселения

        // Конструктор
        public SettlementEvictionService()
        {
            Settlements = new List<ISettlement>();
            Evictions = new List<IEviction>();
        }

        // Конструктор с начальными данными
        public SettlementEvictionService(List<ISettlement> initialSettlements, List<IEviction> initialEvictions)
        {
            Settlements = initialSettlements ?? new List<ISettlement>();
            Evictions = initialEvictions ?? new List<IEviction>();
        }

        // Метод из UML: ДобавитьЗаселение
        public void AddSettlement(ISettlement settlement)
        {
            if (settlement == null)
            {
                throw new ArgumentNullException(nameof(settlement), "Заселение не может быть null");
            }

            // Проверяем, не существует ли уже заселение с таким же ID
            if (Settlements.Any(s => s == settlement))
            {
                throw new InvalidOperationException("Это заселение уже добавлено в сервис");
            }

            // Проверяем, не конфликтует ли это заселение с существующими
            ValidateSettlement(settlement);

            Settlements.Add(settlement);
            Console.WriteLine($"Заселение добавлено в сервис: комната {settlement.Room?.Number}, {settlement.Occupants?.Count ?? 0} жильцов");
        }

        // Метод из UML: ДобавитьВыселение
        public void AddEviction(IEviction eviction)
        {
            if (eviction == null)
            {
                throw new ArgumentNullException(nameof(eviction), "Выселение не может быть null");
            }

            // Проверяем, не существует ли уже выселение с таким же ID
            if (Evictions.Any(e => e == eviction))
            {
                throw new InvalidOperationException("Это выселение уже добавлено в сервис");
            }

            // Проверяем, что связанное заселение существует в сервисе
            if (eviction.RelatedSettlement != null && !Settlements.Contains(eviction.RelatedSettlement))
            {
                AddSettlement(eviction.RelatedSettlement);
            }

            Evictions.Add(eviction);
            Console.WriteLine($"Выселение добавлено в сервис: комната {eviction.Room?.Number}, {eviction.Occupants?.Count ?? 0} жильцов");
        }

        // Дополнительные методы

        public bool RemoveSettlement(Guid settlementId)
        {
            var settlement = Settlements.FirstOrDefault(s =>
                s is Settlement sObj && GetSettlementId(sObj) == settlementId);

            if (settlement == null)
                return false;

            // Проверяем, нет ли связанных выселений
            var relatedEvictions = GetEvictionsForSettlement(settlement);
            if (relatedEvictions.Any())
            {
                // Можно удалить связанные выселения или запретить удаление
                // Решаем удалить все связанные выселения
                foreach (var eviction in relatedEvictions.ToList())
                {
                    RemoveEviction(eviction.Id);
                }
            }

            Settlements.Remove(settlement);
            Console.WriteLine($"Заселение удалено из сервиса");
            return true;
        }

        public bool RemoveEviction(Guid evictionId)
        {
            var eviction = Evictions.FirstOrDefault(e =>
                e is Eviction eObj && GetEvictionId(eObj) == evictionId);

            if (eviction == null)
                return false;

            Evictions.Remove(eviction);
            Console.WriteLine($"Выселение удалено из сервиса");
            return true;
        }

        public ISettlement FindSettlementById(Guid id)
        {
            return Settlements.FirstOrDefault(s =>
                s is Settlement sObj && GetSettlementId(sObj) == id);
        }

        public IEviction FindEvictionById(Guid id)
        {
            return Evictions.FirstOrDefault(e =>
                e is Eviction eObj && GetEvictionId(eObj) == id);
        }

        public List<ISettlement> GetSettlementsForRoom(IRoom room)
        {
            if (room == null)
                return new List<ISettlement>();

            return Settlements
                .Where(s => s.Room != null && s.Room.Number == room.Number)
                .ToList();
        }

        public List<IEviction> GetEvictionsForRoom(IRoom room)
        {
            if (room == null)
                return new List<IEviction>();

            return Evictions
                .Where(e => e.Room != null && e.Room.Number == room.Number)
                .ToList();
        }

        public List<ISettlement> GetActiveSettlements()
        {
            return Settlements
                .Where(s => s.IsActive)
                .ToList();
        }

        public List<IEviction> GetPendingEvictions()
        {
            return Evictions
                .Where(e => e is Eviction eviction &&
                           (eviction.Status == EvictionStatus.Initialized ||
                           eviction.Status == EvictionStatus.InProgress))
                .ToList();
        }

        public List<IRoomOccupant> GetOccupantsInRoom(IRoom room)
        {
            if (room == null)
                return new List<IRoomOccupant>();

            // Получаем всех жильцов из комнаты
            return room.GetAllOccupants();
        }

        public bool IsRoomOccupied(IRoom room)
        {
            if (room == null)
                return false;

            return room.GetAllOccupants().Any();
        }

        public int GetRoomOccupancyCount(IRoom room)
        {
            if (room == null)
                return 0;

            return room.GetAllOccupants().Count;
        }

        public (int TotalSettlements, int TotalEvictions, int ActiveSettlements, int PendingEvictions) GetStatistics()
        {
            int totalSettlements = Settlements.Count;
            int totalEvictions = Evictions.Count;
            int activeSettlements = GetActiveSettlements().Count;
            int pendingEvictions = GetPendingEvictions().Count;

            return (totalSettlements, totalEvictions, activeSettlements, pendingEvictions);
        }

        public string GetServiceInfo()
        {
            var stats = GetStatistics();
            var info = new StringBuilder();

            info.AppendLine("=== СЕРВИС ЗАСЕЛЕНИЙ И ВЫСЕЛЕНИЙ ===");
            info.AppendLine($"Всего заселений: {stats.TotalSettlements}");
            info.AppendLine($"Всего выселений: {stats.TotalEvictions}");
            info.AppendLine($"Активных заселений: {stats.ActiveSettlements}");
            info.AppendLine($"Ожидающих выселений: {stats.PendingEvictions}");

            // Группировка по комнатам
            var rooms = Settlements.Select(s => s.Room)
                .Concat(Evictions.Select(e => e.Room))
                .Where(r => r != null)
                .Distinct()
                .ToList();

            info.AppendLine($"\nКомнаты в обслуживании: {rooms.Count}");

            foreach (var room in rooms.Take(10)) // Ограничим вывод первыми 10 комнатами
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

        // Вспомогательные методы для получения ID
        private Guid GetSettlementId(Settlement settlement)
        {
            // Здесь должна быть логика получения ID из Settlement
            // Так как у нас нет свойства Id в интерфейсе, используем reflection или другой метод
            // Временное решение: используем хэш-код
            return Guid.Empty; // Заглушка - нужно реализовать в реальном проекте
        }

        private Guid GetEvictionId(Eviction eviction)
        {
            // Аналогично для Eviction
            return Guid.Empty; // Заглушка
        }

        // Вспомогательные методы валидации
        private void ValidateSettlement(ISettlement settlement)
        {
            if (settlement.Room == null)
                return;

            // Проверяем, нет ли активных выселений для этой комнаты
            var pendingEvictions = GetPendingEvictionsForRoom(settlement.Room);
            if (pendingEvictions.Any())
            {
                throw new InvalidOperationException(
                    $"Для комнаты {settlement.Room.Number} есть ожидающие выселения. Сначала завершите их.");
            }

            // Проверяем, не переполнена ли комната
            int currentOccupants = GetRoomOccupancyCount(settlement.Room);
            int newOccupants = settlement.Occupants?.Count ?? 0;
            int roomCapacity = GetRoomCapacity(settlement.Room);

            if (currentOccupants + newOccupants > roomCapacity)
            {
                throw new InvalidOperationException(
                    $"Комната {settlement.Room.Number} не может вместить {newOccupants} новых жильцов. " +
                    $"Текущих: {currentOccupants}, вместимость: {roomCapacity}");
            }
        }

        private List<IEviction> GetPendingEvictionsForRoom(IRoom room)
        {
            return Evictions
                .Where(e => e.Room == room &&
                           e is Eviction eviction &&
                          (eviction.Status == EvictionStatus.Initialized ||
                           eviction.Status == EvictionStatus.InProgress))
                .ToList();
        }

        private int GetRoomCapacity(IRoom room)
        {
            // В вашем IRoom есть свойство Type, которое, судя по коду Room, определяет вместимость
            return room?.Type ?? 0;
        }

        private List<IEviction> GetEvictionsForSettlement(ISettlement settlement)
        {
            return Evictions
                .Where(e => e.RelatedSettlement == settlement)
                .ToList();
        }

        // Метод для создания полного цикла: заселение → выселение
        public IEviction CreateEvictionForSettlement(ISettlement settlement, string reason, List<IRoomOccupant> occupantsToEvict = null)
        {
            if (settlement == null)
                throw new ArgumentNullException(nameof(settlement));

            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Причина выселения не может быть пустой", nameof(reason));

            // Если не указаны жильцы для выселения, выселяем всех
            var occupants = occupantsToEvict ?? settlement.Occupants;

            // Создаем выселение
            var eviction = new Eviction();
            eviction.InitializeEviction(
                ID: Guid.NewGuid(),
                occupants: occupants,
                room: settlement.Room,
                reason: reason,
                relatedSettlement: settlement
            );

            // Добавляем в сервис
            AddEviction(eviction);

            return eviction;
        }

        // Метод для выполнения заселения через сервис
        public bool PerformSettlement(ISettlement settlement)
        {
            try
            {
                settlement.PerformSettlement();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при выполнении заселения: {ex.Message}");
                return false;
            }
        }

        // Метод для выполнения выселения через сервис
        public bool PerformEviction(IEviction eviction)
        {
            try
            {
                return eviction.PerformEviction();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при выполнении выселения: {ex.Message}");
                return false;
            }
        }
    }
}
