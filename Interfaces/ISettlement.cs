using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Interfaces
{
    public interface ISettlement
    {
        // Свойства из UML
        Guid Id { get; }
        DateTime SettlementDate { get; } // ДатаЗаселения
        List<IRoomOccupant> Occupants { get; } // Жильцы
        IRoom Room { get; } // Комната
        IDocument Document { get; } // Документ
        bool IsActive { get; } // Активно

        // Методы из UML
        void InitializeSettlement(List<IRoomOccupant> occupants, IRoom room, IDocument document, DateTime date);
        void PerformSettlement();
        void CancelSettlement(string reason);
        void CompleteSettlement();
        string GetSettlementInfo();
    }
}
