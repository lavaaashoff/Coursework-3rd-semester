using System;
using System.Collections.Generic;

namespace CouseWork3Semester.Interfaces
{
    public interface ISettlementEvictionService
    {
        List<ISettlement> Settlements { get; } 
        List<IEviction> Evictions { get; }


        void AddSettlement(ISettlement settlement); 
        void AddEviction(IEviction eviction); 


        bool RemoveSettlement(Guid settlementId);
        bool RemoveEviction(Guid evictionId);
        ISettlement FindSettlementById(Guid id);
        IEviction FindEvictionById(Guid id);
        List<ISettlement> GetSettlementsForRoom(IRoom room);
        List<IEviction> GetEvictionsForRoom(IRoom room);
        List<IRoomOccupant> GetOccupantsInRoom(IRoom room);
        bool IsRoomOccupied(IRoom room);
        int GetRoomOccupancyCount(IRoom room);
        (int TotalSettlements, int TotalEvictions) GetStatistics();
        string GetServiceInfo();
    }
}