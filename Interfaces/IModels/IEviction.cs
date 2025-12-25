using System;
using System.Collections.Generic;

namespace CouseWork3Semester.Interfaces
{
    public interface IEviction
    {
        // Свойства (без статусов)
        Guid Id { get; }
        DateTime EvictionDate { get; } // ДатаВыселения
        string Reason { get; } // Причина
        IRoom Room { get; } // Комната
        List<IRoomOccupant> Occupants { get; } // Жильцы
        ISettlement RelatedSettlement { get; } // СвязанноеЗаселение

        // Методы (инициализация сразу выполняет выселение)
        void InitializeEviction(Guid ID, List<IRoomOccupant> occupants, IRoom room, string reason, ISettlement relatedSettlement);
        string GetEvictionInfo();

        // Дополнительные методы (оставлены как утилитарные)
        bool CanEvictOccupant(IRoomOccupant occupant);
        void AddOccupantToEviction(IRoomOccupant occupant);
        bool RemoveOccupantFromEviction(Guid occupantId);
        bool IsOccupantInRoom(Guid occupantId);
    }
}