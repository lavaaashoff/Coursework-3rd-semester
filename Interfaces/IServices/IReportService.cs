using CouseWork3Semester.Interfaces;
using System;

namespace CouseWork3Semester.Interfaces
{
    public interface IReportService
    {
        IDormitoryRegistry DormitoryRegistry { get; }
        IOccupantRegistry OccupantRegistry { get; }

        string GenerateDormitoryReport();
        string GenerateSettlementReport(DateTime startDate, DateTime endDate);
        string GenerateInventoryReport(IDormitory dormitory);

        string GenerateFreeRoomsReport(IDormitory dormitory = null);
        string GenerateDormResidentsReport(int dormitoryNumber);
    }
}