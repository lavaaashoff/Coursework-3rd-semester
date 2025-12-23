using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Interfaces
{
    public interface ISettlementEvictionService
    {
        // Свойства из UML (через интерфейсы)
        List<ISettlement> Settlements { get; } // Заселения
        List<IEviction> Evictions { get; } // Выселения

        // Методы из UML
        void AddSettlement(ISettlement settlement); // ДобавитьЗаселение
        void AddEviction(IEviction eviction); // ДобавитьВыселение

        // Дополнительные методы
        bool RemoveSettlement(Guid settlementId);
        bool RemoveEviction(Guid evictionId);
        ISettlement FindSettlementById(Guid id);
        IEviction FindEvictionById(Guid id);
        List<ISettlement> GetSettlementsForRoom(IRoom room);
        List<IEviction> GetEvictionsForRoom(IRoom room);
        List<ISettlement> GetActiveSettlements();
        List<IEviction> GetPendingEvictions();
        List<IRoomOccupant> GetOccupantsInRoom(IRoom room);
        bool IsRoomOccupied(IRoom room);
        int GetRoomOccupancyCount(IRoom room);
        (int TotalSettlements, int TotalEvictions, int ActiveSettlements, int PendingEvictions) GetStatistics();
        string GetServiceInfo();
    }
}
