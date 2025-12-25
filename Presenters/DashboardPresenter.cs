using System;
using System.Linq; // ДОБАВЛЕНО
using System.Windows;
using CouseWork3Semester.Interfaces;
using CouseWork3Semester.Views;
using CouseWork3Semester.Services;
using CouseWork3Semester.Registries;
using CouseWork3Semester.Models;
using System.Collections.Generic;

namespace CouseWork3Semester.Presenters
{
    public class DashboardPresenter
    {
        private readonly IAccountingSystem _sys;
        private readonly IStorageService _storage;
        private DashboardView _view;

        public DashboardPresenter(IAccountingSystem accountingSystem, IStorageService storage)
        {
            _sys = accountingSystem ?? throw new ArgumentNullException(nameof(accountingSystem));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        public void InitializeDashboard(DashboardView view)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            var employee = _sys.GetCurrentEmployee();
            var pm = _sys.PermissionManager;

            _view.ManageDormitoriesButton.Visibility =
                pm.CanRolePerformAction(employee.Role, "ManageDormitories") ? Visibility.Visible : Visibility.Collapsed;
            _view.ManageRoomsButton.Visibility =
                pm.CanRolePerformAction(employee.Role, "ManageRooms") ? Visibility.Visible : Visibility.Collapsed;
            _view.ManageOccupantsButton.Visibility =
                pm.CanRolePerformAction(employee.Role, "ManageOccupants") ? Visibility.Visible : Visibility.Collapsed;
            _view.ManageDocumentsButton.Visibility =
                pm.CanRolePerformAction(employee.Role, "ManageDocuments") ? Visibility.Visible : Visibility.Collapsed;
            _view.ManageEvictionsButton.Visibility =
                pm.CanRolePerformAction(employee.Role, "ManageEvictions") ? Visibility.Visible : Visibility.Collapsed;
            _view.ManageInventoryButton.Visibility =
                pm.CanRolePerformAction(employee.Role, "ManageInventory") ? Visibility.Visible : Visibility.Collapsed;
            _view.ReportsButton.Visibility =
                pm.CanRolePerformAction(employee.Role, "GenerateReports") ? Visibility.Visible : Visibility.Collapsed;

            _view.ManageDormitoriesButton.Click += (s, e) =>
            {
                var v = new DormitoriesView();
                var p = new DormitoriesPresenter(v, _sys);
                v.Show();
            };

            _view.ManageRoomsButton.Click += (s, e) =>
            {
                var v = new RoomsView();
                var p = new RoomsPresenter(v, _sys);
                v.Show();
            };

            _view.ManageOccupantsButton.Click += (s, e) =>
            {
                var v = new ResidentsView();
                var p = new ResidentsPresenter(v, _sys);
                v.Show();
            };

            _view.ManageDocumentsButton.Click += (s, e) =>
            {
                var v = new DocumentsView();
                var p = new DocumentsPresenter(v, _sys);
                v.Show();
            };

            _view.ManageSettlementsButton.Click += (s, e) =>
            {
                var v = new SettlementsView();
                var p = new SettlementsPresenter(v, _sys);
                v.Show();
            };

            _view.ManageEvictionsButton.Click += (s, e) =>
            {
                var v = new EvictionsView();
                var p = new EvictionsPresenter(v, _sys);
                v.Show();
            };

            _view.ManageInventoryButton.Click += (s, e) =>
            {
                var v = new InventoryView();
                var p = new InventoryPresenter(v, _sys);
                v.Show();
            };

            _view.ReportsButton.Click += (s, e) =>
            {
                var v = new ReportsSearchView();
                var p = new ReportsSearchPresenter(v, _sys);
                v.Show();
            };

            _view.LogoutButton.Click += (s, e) =>
            {
                try
                {
                    _storage.Save(_sys);
                    _sys.AuthManager.Logout();

                    var loginView = new LoginView();
                    Application.Current.MainWindow = loginView;

                    var loginPresenter = new LoginPresenter(_sys.AuthManager, loginView, newEmployee =>
                    {
                        DormitoryRegistry dormitoryRegistry;
                        OccupantRegistry occupantRegistry;
                        DocumentRegistry documentRegistry;
                        SettlementEvictionService settlementEvictionService;
                        InventoryRegistry inventoryRegistry;
                        List<DocumentOccupantLink> docLinks;

                        if ((_storage as JsonStorageService)?.TryLoad(out var state) == true && state != null)
                        {
                            dormitoryRegistry = state.DormitoryRegistry ?? new DormitoryRegistry();
                            occupantRegistry = state.OccupantRegistry ?? new OccupantRegistry();
                            documentRegistry = state.DocumentRegistry ?? new DocumentRegistry();
                            settlementEvictionService = state.SettlementEvictionService ?? new SettlementEvictionService();
                            inventoryRegistry = state.InventoryRegistry ?? new InventoryRegistry();
                            docLinks = state.DocumentLinks ?? new List<DocumentOccupantLink>();
                        }
                        else
                        {
                            dormitoryRegistry = _sys.DormitoryRegistry as DormitoryRegistry ?? new DormitoryRegistry();
                            occupantRegistry = _sys.OccupantRegistry as OccupantRegistry ?? new OccupantRegistry();
                            documentRegistry = _sys.DocumentRegistry as DocumentRegistry ?? new DocumentRegistry();
                            settlementEvictionService = _sys.SettlementEvictionService as SettlementEvictionService ?? new SettlementEvictionService();
                            inventoryRegistry = _sys.InventoryRegistry as InventoryRegistry ?? new InventoryRegistry();
                            docLinks = _sys.DocumentOccupantService?.GetAllLinks()?.OfType<DocumentOccupantLink>()?.ToList() ?? new List<DocumentOccupantLink>();
                        }

                        var reportService = new ReportService(dormitoryRegistry, occupantRegistry);
                        var searchService = new SearchService(dormitoryRegistry, occupantRegistry);

                        // ИСПРАВЛЕНО: приводим список к интерфейсному типу
                        var documentOccupantService = new DocumentOccupantService(
                            documentRegistry,
                            occupantRegistry,
                            _sys.DocumentValidator,
                            docLinks.Cast<IDocumentOccupantLink>().ToList()
                        );

                        var newSys = new Services.AccountingSystem(
                            dormitoryRegistry,
                            occupantRegistry,
                            settlementEvictionService,
                            reportService,
                            searchService,
                            _sys.AuthManager,
                            _sys.PermissionManager,
                            documentOccupantService,
                            _sys.PassportValidator,
                            _sys.DocumentValidator,
                            currentEmployee: newEmployee,
                            documentRegistry: documentRegistry,
                            inventoryRegistry: inventoryRegistry
                        );

                        var dash = new DashboardView();
                        var dashPresenter = new DashboardPresenter(newSys, _storage);
                        dashPresenter.InitializeDashboard(dash);
                        Application.Current.MainWindow = dash;
                        dash.Show();
                    });

                    loginView.Show();
                    _view.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Logout failed: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };
        }
    }
}