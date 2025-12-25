using CouseWork3Semester.Interfaces;
using System;
using System.Collections.Generic;

namespace CouseWork3Semester.Interfaces
{
    public interface ISettlement
    {
        Guid Id { get; }
        DateTime SettlementDate { get; } 
        List<IRoomOccupant> Occupants { get; } 
        IRoom Room { get; } 
        IDocument Document { get; } 

        void InitializeSettlement(List<IRoomOccupant> occupants, IRoom room, IDocument document, DateTime date);
        string GetSettlementInfo();
    }
}