using CouseWork3Semester.Interfaces;
using System;
using System.Collections.Generic;

namespace CouseWork3Semester.Interfaces
{
    public interface ISettlement
    {
        // Свойства (без статусов и флага активности)
        Guid Id { get; }
        DateTime SettlementDate { get; } // ДатаЗаселения
        List<IRoomOccupant> Occupants { get; } // Жильцы
        IRoom Room { get; } // Комната
        IDocument Document { get; } // Документ

        // Методы (упрощённая логика: инициализация сразу выполняет заселение)
        void InitializeSettlement(List<IRoomOccupant> occupants, IRoom room, IDocument document, DateTime date);
        string GetSettlementInfo();
    }
}