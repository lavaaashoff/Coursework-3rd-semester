using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Interfaces
{
    public interface IPaymentService
    {
        // Свойства
        List<IPayment> Payments { get; }

        // Основные методы из UML
        IPayment ChargePayment(IRoomOccupant occupant, DateTime period, decimal amount);
        bool AcceptPayment(IPayment payment, DateTime paymentDate);
        List<IPayment> GetPaymentHistory(IRoomOccupant occupant);
        decimal CalculateTotalDebt(IRoomOccupant occupant);
    }
}
