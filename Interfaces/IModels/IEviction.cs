using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Interfaces
{
    public interface IEviction
    {
        // Свойства из UML
        Guid Id { get; }
        DateTime EvictionDate { get; } // ДатаВыселения
        string Reason { get; } // Причина
        IRoom Room { get; } // Комната
        List<IRoomOccupant> Occupants { get; } // Жильцы
        ISettlement RelatedSettlement { get; } // СвязанноеЗаселение

        // Методы из UML
        void InitializeEviction(Guid ID, List<IRoomOccupant> occupants, IRoom room, string reason, ISettlement relatedSettlement);
        bool PerformEviction();
        string GetEvictionInfo();

        // Дополнительные методы
        bool CanEvictOccupant(IRoomOccupant occupant);
        void AddOccupantToEviction(IRoomOccupant occupant);
        bool RemoveOccupantFromEviction(Guid occupantId);
        bool IsOccupantInRoom(Guid occupantId);
    }
}
