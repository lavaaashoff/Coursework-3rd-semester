using System;
using System.Collections.Generic;

namespace CouseWork3Semester.Interfaces
{
    public interface IEviction
    {
        Guid Id { get; }
        DateTime EvictionDate { get; } 
        string Reason { get; } 
        IRoom Room { get; } 
        List<IRoomOccupant> Occupants { get; } 
        ISettlement RelatedSettlement { get; } 

        void InitializeEviction(Guid ID, List<IRoomOccupant> occupants, IRoom room, string reason, ISettlement relatedSettlement);
        string GetEvictionInfo();

        bool CanEvictOccupant(IRoomOccupant occupant);
        void AddOccupantToEviction(IRoomOccupant occupant);
        bool RemoveOccupantFromEviction(Guid occupantId);
        bool IsOccupantInRoom(Guid occupantId);
    }
}