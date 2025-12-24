using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Interfaces
{
    public interface IPayment
    {
        // Свойства из UML
        IRoomOccupant Occupant { get; }          // Жилец
        DateTime AssignmentDate { get; }         // ДатаНазначения
        decimal Amount { get; }                  // Сумма (фиксированная)
        DateTime? PaymentDate { get; }           // ДатаОплаты (null если не оплачено)
        bool Status { get; }                     // Статус (true = оплачено, false = не оплачено)

        // Методы из UML
        void ConfirmPayment(DateTime paymentDate);   // ПодтвердитьОплату

        // Дополнительные методы
        string GetPaymentInfo();                      // Получить информацию об оплате      
    }
}
