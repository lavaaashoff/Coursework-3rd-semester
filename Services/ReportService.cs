using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CouseWork3Semester.Interfaces;

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

        public string GenerateInventoryReport(IDormitory dormitory)
        {
            var report = new StringBuilder();
            report.AppendLine("ОТЧЕТ ПО ИНВЕНТАРЮ");
            report.AppendLine($"Дата: {DateTime.Now:dd.MM.yyyy}");

            IEnumerable<IDormitory> dorms = dormitory != null
                ? new[] { dormitory }
                : DormitoryRegistry.GetAllDormitories();

            foreach (var d in dorms.OrderBy(x => x.Number))
            {
                report.AppendLine();
                report.AppendLine($"Общежитие №{d.Number} — {d.Address}");
                report.AppendLine($"Комнат: {d.GetAllRooms().Count}");
                // Здесь можно дополнить данными из InventoryRegistry, если нужно формировать отчёт по предметам
            }

            return report.ToString();
        }

        public string GenerateFreeRoomsReport(IDormitory dormitory = null)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ОТЧЁТ: Список свободных комнат");
            sb.AppendLine($"Дата: {DateTime.Now:dd.MM.yyyy}");
            sb.AppendLine();

            IEnumerable<IDormitory> dorms = dormitory != null
                ? new[] { dormitory }
                : DormitoryRegistry.GetAllDormitories();

            int totalFreeRooms = 0;

            foreach (var d in dorms.OrderBy(x => x.Number))
            {
                var freeRooms = d.GetAllRooms()
                    .Where(r => r.GetAvailablePlacesCount() > 0)
                    .OrderBy(r => r.Number)
                    .ToList();

                sb.AppendLine($"Общежитие №{d.Number} — {d.Address}");
                if (freeRooms.Any())
                {
                    foreach (var r in freeRooms)
                    {
                        sb.AppendLine($"   Комната №{r.Number} (этаж {r.Floor}), свободно мест: {r.GetAvailablePlacesCount()} из {r.Type}");
                    }
                    totalFreeRooms += freeRooms.Count;
                }
                else
                {
                    sb.AppendLine("   Свободных комнат нет");
                }
                sb.AppendLine();
            }

            sb.AppendLine($"Итого свободных комнат: {totalFreeRooms}");
            return sb.ToString();
        }

        public string GenerateDormResidentsReport(int dormitoryNumber)
        {
            var dorm = DormitoryRegistry.GetDormitoryByNumber(dormitoryNumber);
            if (dorm == null)
            {
                return $"Общежитие №{dormitoryNumber} не найдено.";
            }

            var sb = new StringBuilder();
            sb.AppendLine($"ОТЧЁТ: Список проживающих в общежитии №{dormitoryNumber}");
            sb.AppendLine($"Адрес: {dorm.Address}");
            sb.AppendLine($"Дата: {DateTime.Now:dd.MM.yyyy}");
            sb.AppendLine();

            var rooms = dorm.GetAllRooms().OrderBy(r => r.Number).ToList();
            int totalOccupants = 0;

            foreach (var room in rooms)
            {
                var occupants = room.GetAllOccupants();
                if (occupants.Any())
                {
                    sb.AppendLine($"Комната №{room.Number} (этаж {room.Floor}) — проживает: {occupants.Count}");
                    foreach (var occ in occupants.OrderBy(o => o.FullName))
                    {
                        sb.AppendLine($"   - {occ.FullName} ({occ.GetAge()} лет, тип: {occ.GetOccupantType()})");
                        totalOccupants++;
                    }
                }
            }

            if (totalOccupants == 0)
            {
                sb.AppendLine("Проживающих нет.");
            }

            sb.AppendLine();
            sb.AppendLine($"ИТОГО: проживающих — {totalOccupants}");
            return sb.ToString();
        }
    }
}