using System;
using System.Windows;
using CouseWork3Semester.Interfaces;
using CouseWork3Semester.Views;

namespace CouseWork3Semester.Presenters
{
    public class DashboardPresenter
    {
        private readonly IAccountingSystem _sys;
        private DashboardView _view;

        public DashboardPresenter(IAccountingSystem accountingSystem)
        {
            _sys = accountingSystem ?? throw new ArgumentNullException(nameof(accountingSystem));
        }

        public void InitializeDashboard(DashboardView view)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));

            var employee = _sys.GetCurrentEmployee();
            var pm = _sys.PermissionManager;

            // Видимость по ролям
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

            // Навигация: Справочник общежитий
            _view.ManageDormitoriesButton.Click += (s, e) =>
            {
                var v = new DormitoriesView();
                var p = new DormitoriesPresenter(v, _sys);
                v.Show();
            };

            // Навигация: Учёт комнат
            _view.ManageRoomsButton.Click += (s, e) =>
            {
                var v = new RoomsView();
                var p = new RoomsPresenter(v, _sys);
                v.Show();
            };

            // Навигация: Регистрация жильцов
            _view.ManageOccupantsButton.Click += (s, e) =>
            {
                var v = new ResidentsView();
                var p = new ResidentsPresenter(v, _sys);
                v.Show();
            };

            // Навигация: Учёт документов
            _view.ManageDocumentsButton.Click += (s, e) =>
            {
                var v = new DocumentsView();
                var p = new DocumentsPresenter(v, _sys);
                v.Show();
            };

            // Новая навигация: Менеджер заселений
            _view.ManageSettlementsButton.Click += (s, e) =>
            {
                var v = new SettlementsView();
                var p = new SettlementsPresenter(v, _sys);
                v.Show();
            };

            // Новая навигация: Менеджер выселений
            _view.ManageEvictionsButton.Click += (s, e) =>
            {
                var v = new EvictionsView();
                var p = new EvictionsPresenter(v, _sys);
                v.Show();
            };

            // Менеджер инвентаря
            _view.ManageInventoryButton.Click += (s, e) =>
            {
                var v = new InventoryView();
                var p = new InventoryPresenter(v, _sys);
                v.Show();
            };

            // Logout
            _view.LogoutButton.Click += (s, e) =>
            {
                try
                {
                    _sys.AuthManager.Logout();

                    var loginView = new LoginView();
                    Application.Current.MainWindow = loginView;

                    var loginPresenter = new LoginPresenter(_sys.AuthManager, loginView, newEmployee =>
                    {
                        // Пересобираем систему с новым текущим сотрудником
                        var newSys = new Services.AccountingSystem(
                            _sys.DormitoryRegistry,
                            _sys.OccupantRegistry,
                            _sys.SettlementEvictionService,
                            _sys.PaymentService,
                            _sys.ReportService,
                            _sys.SearchService,
                            _sys.AuthManager,
                            _sys.PermissionManager,
                            _sys.DocumentOccupantService,
                            _sys.PassportValidator,
                            _sys.DocumentValidator,
                            currentEmployee: newEmployee,
                            documentRegistry: _sys.DocumentRegistry,
                            inventoryRegistry: _sys.InventoryRegistry
                        );

                        var dash = new DashboardView();
                        var dashPresenter = new DashboardPresenter(newSys);
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