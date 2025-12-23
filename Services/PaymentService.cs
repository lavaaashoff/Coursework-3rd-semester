using CouseWork3Semester.Interfaces;
using CouseWork3Semester.Interfaces.IModels;
using CouseWork3Semester.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Services
{
    public class PaymentService : IPaymentService
    {
        // Свойство для списка платежей
        public List<IPayment> Payments { get; private set; }

        // Конструктор
        public PaymentService()
        {
            Payments = new List<IPayment>();
        }

        // Конструктор с начальным списком платежей
        public PaymentService(List<IPayment> initialPayments)
        {
            if (initialPayments == null)
                throw new ArgumentNullException(nameof(initialPayments));

            Payments = initialPayments;
        }

        // Реализация методов интерфейса

        public IPayment ChargePayment(IRoomOccupant occupant, DateTime period, decimal amount)
        {
            if (occupant == null)
                throw new ArgumentNullException(nameof(occupant), "Жилец не может быть null");

            if (amount <= 0)
                throw new ArgumentException("Сумма должна быть положительной", nameof(amount));

            if (period > DateTime.Now.AddDays(365))
                throw new ArgumentException("Период не может быть больше чем на год вперед", nameof(period));

            // Проверяем, нет ли уже назначенной оплаты на этот период для этого жильца
            var existingPayment = Payments.FirstOrDefault(p =>
                p.Occupant.Id == occupant.Id &&
                p.AssignmentDate.Date == period.Date);

            if (existingPayment != null)
            {
                throw new InvalidOperationException($"Для жильца {occupant.FullName} уже назначена оплата на период {period:dd.MM.yyyy}");
            }

            // Создаем новую оплату
            var payment = new Payment(occupant, amount, period);
            Payments.Add(payment);

            Console.WriteLine($"Начислена оплата для {occupant.FullName}: {amount:C} за период {period:dd.MM.yyyy}");
            return payment;
        }

        public bool AcceptPayment(IPayment payment, DateTime paymentDate)
        {
            if (payment == null)
                throw new ArgumentNullException(nameof(payment), "Платеж не может быть null");

            if (paymentDate > DateTime.Now)
                throw new ArgumentException("Дата оплаты не может быть в будущем", nameof(paymentDate));

            // Проверяем, существует ли такой платеж в системе
            if (!Payments.Contains(payment))
            {
                throw new InvalidOperationException("Платеж не найден в системе");
            }

            // Проверяем, не оплачен ли уже этот платеж
            if (payment.Status)
            {
                Console.WriteLine($"Платеж уже был оплачен ранее: {payment.GetPaymentInfo()}");
                return false;
            }

            try
            {
                payment.ConfirmPayment(paymentDate);
                Console.WriteLine($"Оплата принята: {payment.Occupant.FullName}, {payment.Amount:C}, {paymentDate:dd.MM.yyyy}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при принятии оплаты: {ex.Message}");
                return false;
            }
        }

        public List<IPayment> GetPaymentHistory(IRoomOccupant occupant)
        {
            if (occupant == null)
                throw new ArgumentNullException(nameof(occupant), "Жилец не может быть null");

            return Payments
                .Where(p => p.Occupant.Id == occupant.Id)
                .OrderByDescending(p => p.AssignmentDate)
                .ToList();
        }

        public decimal CalculateTotalDebt(IRoomOccupant occupant)
        {
            if (occupant == null)
                throw new ArgumentNullException(nameof(occupant), "Жилец не может быть null");

            // Суммируем все неоплаченные платежи
            decimal totalDebt = Payments
                .Where(p => p.Occupant.Id == occupant.Id && !p.Status)
                .Sum(p => p.Amount);

            Console.WriteLine($"Общая задолженность {occupant.FullName}: {totalDebt:C}");
            return totalDebt;
        }

        // Дополнительные методы (не из интерфейса, но полезные)

        public decimal CalculateTotalRevenue()
        {
            decimal totalRevenue = Payments
                .Where(p => p.Status)
                .Sum(p => p.Amount);

            Console.WriteLine($"Общая выручка: {totalRevenue:C}");
            return totalRevenue;
        }

        public List<IPayment> GetOverduePayments(DateTime currentDate)
        {
            return Payments
                .Where(p => !p.Status && p.AssignmentDate < currentDate)
                .OrderBy(p => p.AssignmentDate)
                .ToList();
        }

        public List<IPayment> GetPendingPayments()
        {
            return Payments
                .Where(p => !p.Status)
                .OrderBy(p => p.AssignmentDate)
                .ToList();
        }

        public List<IPayment> GetPaidPayments()
        {
            return Payments
                .Where(p => p.Status)
                .OrderByDescending(p => p.PaymentDate)
                .ToList();
        }

        public decimal CalculateTotalDebtForPeriod(DateTime startDate, DateTime endDate)
        {
            decimal totalDebt = Payments
                .Where(p => !p.Status &&
                           p.AssignmentDate >= startDate &&
                           p.AssignmentDate <= endDate)
                .Sum(p => p.Amount);

            Console.WriteLine($"Общая задолженность за период {startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}: {totalDebt:C}");
            return totalDebt;
        }

        public List<IPayment> GetPaymentsForPeriod(DateTime startDate, DateTime endDate)
        {
            return Payments
                .Where(p => p.AssignmentDate >= startDate && p.AssignmentDate <= endDate)
                .OrderBy(p => p.AssignmentDate)
                .ToList();
        }

        public Dictionary<IRoomOccupant, decimal> GetDebtorsList()
        {
            var debtors = new Dictionary<IRoomOccupant, decimal>();

            foreach (var payment in Payments.Where(p => !p.Status))
            {
                if (!debtors.ContainsKey(payment.Occupant))
                {
                    debtors[payment.Occupant] = 0;
                }
                debtors[payment.Occupant] += payment.Amount;
            }

            return debtors
                .OrderByDescending(kv => kv.Value)
                .ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        public List<IPayment> GetPaymentsByDate(DateTime date)
        {
            return Payments
                .Where(p => p.AssignmentDate.Date == date.Date)
                .OrderBy(p => p.Occupant.FullName)
                .ToList();
        }

        public bool CancelPayment(IPayment payment)
        {
            if (payment == null)
                throw new ArgumentNullException(nameof(payment), "Платеж не может быть null");

            if (!Payments.Contains(payment))
            {
                throw new InvalidOperationException("Платеж не найден в системе");
            }

            try
            {
                // Если это Payment из вашей реализации
                if (payment is Payment concretePayment)
                {
                    concretePayment.CancelPayment();
                    return true;
                }
                else
                {
                    Console.WriteLine("Не удалось отменить платеж: неподдерживаемый тип");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отмене платежа: {ex.Message}");
                return false;
            }
        }

        public void ClearPayments()
        {
            // Можно очищать только оплаченные платежи
            var paidPayments = GetPaidPayments();
            foreach (var payment in paidPayments)
            {
                Payments.Remove(payment);
            }

            Console.WriteLine($"Очищено {paidPayments.Count} оплаченных платежей");
        }

        // Статистические отчеты

        public string GetPaymentStatistics()
        {
            var stats = new System.Text.StringBuilder();

            int totalPayments = Payments.Count;
            int paidPayments = GetPaidPayments().Count;
            int pendingPayments = GetPendingPayments().Count;
            decimal totalRevenue = CalculateTotalRevenue();
            decimal totalDebt = Payments.Where(p => !p.Status).Sum(p => p.Amount);

            stats.AppendLine("СТАТИСТИКА ОПЛАТ");
            stats.AppendLine("================");
            stats.AppendLine($"Всего платежей: {totalPayments}");
            stats.AppendLine($"Оплачено: {paidPayments} ({((double)paidPayments / totalPayments * 100):F1}%)");
            stats.AppendLine($"Не оплачено: {pendingPayments} ({((double)pendingPayments / totalPayments * 100):F1}%)");
            stats.AppendLine($"Общая выручка: {totalRevenue:C}");
            stats.AppendLine($"Общая задолженность: {totalDebt:C}");
            stats.AppendLine();

            // Должники
            var debtors = GetDebtorsList();
            if (debtors.Any())
            {
                stats.AppendLine("СПИСОК ДОЛЖНИКОВ:");
                foreach (var debtor in debtors)
                {
                    stats.AppendLine($"  {debtor.Key.FullName}: {debtor.Value:C}");
                }
            }

            return stats.ToString();
        }

        public override string ToString()
        {
            return $"Payment Service: {Payments.Count} платежей, " +
                   $"{GetPaidPayments().Count} оплачено, " +
                   $"{GetPendingPayments().Count} ожидает оплаты";
        }
    }
}
