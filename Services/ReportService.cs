using CouseWork3Semester.Interfaces.IModels;
using CouseWork3Semester.Interfaces.IRegistries;
using CouseWork3Semester.Interfaces.IServices;
using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Services
{
    public class ReportService : IReportService
    {
        public IDormitoryRegistry DormitoryRegistry { get; }
        public IOccupantRegistry OccupantRegistry { get; }
        public IPaymentService PaymentService { get; }

        public ReportService(
            IDormitoryRegistry dormitoryRegistry,
            IOccupantRegistry occupantRegistry,
            IPaymentService paymentService)
        {
            DormitoryRegistry = dormitoryRegistry ?? throw new ArgumentNullException(nameof(dormitoryRegistry));
            OccupantRegistry = occupantRegistry ?? throw new ArgumentNullException(nameof(occupantRegistry));
            PaymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
        }

        public string GenerateDormitoryReport()
        {
            var report = new StringBuilder();
            var dormitories = DormitoryRegistry.GetAllDormitories();

            report.AppendLine("ОТЧЕТ ПО ОБЩЕЖИТИЯМ");
            report.AppendLine($"Дата: {DateTime.Now:dd.MM.yyyy}");
            report.AppendLine($"Общежитий: {dormitories.Count}");
            report.AppendLine($"Мест всего/занято/свободно: {DormitoryRegistry.GetTotalPlacesCount()}/{DormitoryRegistry.GetTotalOccupiedPlacesCount()}/{DormitoryRegistry.GetTotalAvailablePlacesCount()}");
            report.AppendLine($"Загрузка: {DormitoryRegistry.GetOverallOccupancyPercentage():F1}%");
            report.AppendLine();

            foreach (var dorm in dormitories)
            {
                report.AppendLine($"{dorm.Number}. {dorm.Address}");
                report.AppendLine($"   Комнат: {dorm.GetAllRooms().Count}, Мест: {dorm.GetTotalPlacesCount()}");
                report.AppendLine($"   Занято: {dorm.GetOccupiedPlacesCount()}, Загрузка: {dorm.GetOccupancyPercentage():F1}%");
            }

            return report.ToString();
        }

        public string GenerateSettlementReport(DateTime startDate, DateTime endDate)
        {
            var report = new StringBuilder();
            var residents = OccupantRegistry.GetAllResidents();
            var children = OccupantRegistry.GetAllChildren();

            report.AppendLine("ОТЧЕТ ПО ЗАСЕЛЕНИЯМ");
            report.AppendLine($"Период: {startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}");
            report.AppendLine($"Всего: {OccupantRegistry.GetOccupantsCount()} (Взрослых: {residents.Count}, Детей: {children.Count})");
            report.AppendLine();

            foreach (var resident in residents.OrderBy(r => r.FullName))
            {
                var residentChildren = OccupantRegistry.GetChildrenOfResident(resident.Id);
                report.AppendLine($"{resident.FullName} ({resident.GetAge()} лет)");

                if (residentChildren.Any())
                {
                    report.AppendLine($"   Дети: {string.Join(", ", residentChildren.Select(c => c.FullName))}");
                }
            }

            return report.ToString();
        }

        public string GeneratePaymentReport(IRoomOccupant occupant, DateTime period)
        {
            var report = new StringBuilder();
            var payments = PaymentService.GetPaymentHistory(occupant);
            var periodPayments = payments.Where(p => p.AssignmentDate.Month == period.Month && p.AssignmentDate.Year == period.Year).ToList();

            report.AppendLine("ОТЧЕТ ПО ОПЛАТАМ");
            report.AppendLine($"Жилец: {occupant.FullName}");
            report.AppendLine($"Период: {period:MM.yyyy}");
            report.AppendLine($"Всего платежей: {periodPayments.Count}");
            report.AppendLine($"Оплачено: {periodPayments.Where(p => p.Status).Sum(p => p.Amount):C}");
            report.AppendLine($"Задолженность: {periodPayments.Where(p => !p.Status).Sum(p => p.Amount):C}");
            report.AppendLine();

            foreach (var payment in periodPayments.OrderBy(p => p.AssignmentDate))
            {
                report.AppendLine($"{payment.AssignmentDate:dd.MM.yyyy}: {payment.Amount:C} - {(payment.Status ? "Оплачено" : "Не оплачено")}");
            }

            return report.ToString();
        }

        public string GenerateInventoryReport(IDormitory dormitory)
        {
            var report = new StringBuilder();
            var rooms = dormitory.GetAllRooms();

            report.AppendLine("ОТЧЕТ ПО ИНВЕНТАРЮ");
            report.AppendLine($"Общежитие: {dormitory.Number}, {dormitory.Address}");
            report.AppendLine($"Комнат: {rooms.Count}");
            report.AppendLine();

            foreach (var room in rooms.OrderBy(r => r.Number))
            {
                var occupants = room.GetAllOccupants();
                report.AppendLine($"Комната {room.Number} ({room.Type}-местная, этаж {room.Floor})");
                report.AppendLine($"   Проживает: {occupants.Count}/{room.Type}");

                if (occupants.Any())
                {
                    report.AppendLine($"   Жильцы: {string.Join(", ", occupants.Select(o => o.FullName))}");
                }
            }

            return report.ToString();
        }
    }
}
