using CouseWork3Semester.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Models
{
    public class Payment : IPayment
    {
        // Свойства из UML (через интерфейсы)
        public IRoomOccupant Occupant { get; private set; }          // Жилец
        public DateTime AssignmentDate { get; private set; }         // ДатаНазначения
        public decimal Amount { get; private set; }                  // Сумма (фиксированная)
        public DateTime? PaymentDate { get; private set; }           // ДатаОплаты
        public bool Status { get; private set; }                     // Статус

        // Конструктор
        public Payment(
            IRoomOccupant occupant,
            decimal amount,
            DateTime assignmentDate)
        {
            Validate(occupant, amount, assignmentDate);

            Occupant = occupant;
            Amount = amount;
            AssignmentDate = assignmentDate.Date;
            PaymentDate = null;
            Status = false;

            Console.WriteLine($"Создана оплата для {occupant.FullName}: {amount:C}");
        }

        // Метод из UML: ПодтвердитьОплату
        public void ConfirmPayment(DateTime paymentDate)
        {
            ValidatePaymentDate(paymentDate);

            PaymentDate = paymentDate;
            Status = true;

            Console.WriteLine($"Оплата подтверждена: {Occupant.FullName}, {Amount:C}");
        }

        // Дополнительные методы

        public string GetPaymentInfo()
        {
            var info = new StringBuilder();

            info.AppendLine("=== ОПЛАТА ===");
            info.AppendLine($"Жилец: {Occupant.FullName}");
            info.AppendLine($"Сумма: {Amount:C}");
            info.AppendLine($"Дата назначения: {AssignmentDate:dd.MM.yyyy}");
            info.AppendLine($"Статус: {(Status ? "ОПЛАЧЕНО" : "НЕ ОПЛАЧЕНО")}");

            if (Status && PaymentDate.HasValue)
            {
                info.AppendLine($"Дата оплаты: {PaymentDate.Value:dd.MM.yyyy}");
            }

            if (!Status)
            {
                info.AppendLine($"Задолженность: {Amount:C}");
            }

            info.AppendLine("==============");

            return info.ToString();
        }

        // Вспомогательные методы валидации

        private void Validate(IRoomOccupant occupant, decimal amount, DateTime assignmentDate)
        {
            if (occupant == null)
                throw new ArgumentNullException(nameof(occupant), "Жилец не может быть null");

            if (amount <= 0)
                throw new ArgumentException("Сумма должна быть положительной", nameof(amount));

            if (assignmentDate > DateTime.Now.AddDays(365))
                throw new ArgumentException("Дата назначения не может быть больше чем на год вперед", nameof(assignmentDate));
        }

        private void ValidatePaymentDate(DateTime paymentDate)
        {
            if (paymentDate > DateTime.Now)
                throw new ArgumentException("Дата оплаты не может быть в будущем", nameof(paymentDate));

            if (paymentDate < AssignmentDate.AddMonths(-6))
                throw new ArgumentException("Дата оплаты не может быть раньше чем за 6 месяцев до назначения", nameof(paymentDate));
        }

        // Методы для обновления информации (только если не оплачено)

        public void UpdateAmount(decimal newAmount)
        {
            if (Status)
                throw new InvalidOperationException("Нельзя изменить сумму уже оплаченного платежа");

            if (newAmount <= 0)
                throw new ArgumentException("Сумма должна быть положительной", nameof(newAmount));

            Amount = newAmount;
            Console.WriteLine($"Сумма оплаты обновлена: {newAmount:C}");
        }

        // Отмена оплаты (сброс статуса)
        public void CancelPayment()
        {
            if (!Status)
                throw new InvalidOperationException("Нельзя отменить неоплаченный платеж");

            PaymentDate = null;
            Status = false;

            Console.WriteLine($"Оплата отменена для {Occupant.FullName}");
        }

        // Проверка, назначена ли оплата на конкретную дату
        public bool IsAssignedForDate(DateTime date)
        {
            return AssignmentDate.Date == date.Date;
        }

        // Проверка, оплачена ли в конкретную дату
        public bool WasPaidOnDate(DateTime date)
        {
            return Status && PaymentDate.HasValue && PaymentDate.Value.Date == date.Date;
        }
    }
}
