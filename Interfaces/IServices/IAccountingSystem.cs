using CouseWork3Semester.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Interfaces
{
    public interface IAccountingSystem
    {
        IDormitoryRegistry DormitoryRegistry { get; }
        IOccupantRegistry OccupantRegistry { get; }
        ISettlementEvictionService SettlementEvictionService { get; }
        IReportService ReportService { get; }
        ISearchService SearchService { get; }
        IEmployee CurrentEmployee { get; }
        IAuthManager AuthManager { get; }
        IPermissionManager PermissionManager { get; }
        IDocumentOccupantService DocumentOccupantService { get; }
        IPassportValidator PassportValidator { get; }
        IDocumentValidator DocumentValidator { get; }
        IDocumentRegistry DocumentRegistry { get; }
        IInventoryRegistry InventoryRegistry { get; }

        void RegisterOccupant(IRoomOccupant occupant, IRoom room, IDocument document);
        void EvictOccupant(IRoomOccupant occupant, string reason);
        string GetReport(string reportType, Dictionary<string, object> parameters);
        object Search(string searchType, string query);
        IEmployee GetCurrentEmployee();
    }
}