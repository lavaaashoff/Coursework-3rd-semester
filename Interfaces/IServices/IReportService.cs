using CouseWork3Semester.Interfaces;
using System;

namespace CouseWork3Semester.Interfaces
{
    public interface IReportService
    {
        IDormitoryRegistry DormitoryRegistry { get; }
        IOccupantRegistry OccupantRegistry { get; }
        IPaymentService PaymentService { get; }

        string GenerateDormitoryReport();
        string GenerateSettlementReport(DateTime startDate, DateTime endDate);
        string GenerateInventoryReport(IDormitory dormitory);

        // Новые отчёты (без оплаты)
        // Список свободных комнат (если dormitory == null, по всем)
        string GenerateFreeRoomsReport(IDormitory dormitory = null);

        // Список проживающих в общежитии №...
        string GenerateDormResidentsReport(int dormitoryNumber);
    }
}