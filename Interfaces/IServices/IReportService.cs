using CouseWork3Semester.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Interfaces
{
    public interface IReportService
    {
        IDormitoryRegistry DormitoryRegistry { get; }
        IOccupantRegistry OccupantRegistry { get; }
        IPaymentService PaymentService { get; }

        string GenerateDormitoryReport();
        string GenerateSettlementReport(DateTime startDate, DateTime endDate);
        string GeneratePaymentReport(IRoomOccupant occupant, DateTime period);
        string GenerateInventoryReport(IDormitory dormitory);
    }
}
