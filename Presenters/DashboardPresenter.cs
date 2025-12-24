using System;
using System.Windows;
using CouseWork3Semester.Interfaces;
using CouseWork3Semester.Views;

namespace CouseWork3Semester.Presenters
{
    public class DashboardPresenter
    {
        private readonly IEmployee _currentEmployee;
        private readonly IPermissionManager _permissionManager;
        private readonly IDormitoryRegistry _dormitoryRegistry;
        private readonly IOccupantRegistry _occupantRegistry;
        private readonly IPassportValidator _passportValidator;
        private readonly IAuthManager _authManager;

        private DashboardView _view;

        public DashboardPresenter(
            IEmployee currentEmployee,
            IPermissionManager permissionManager,
            IDormitoryRegistry dormitoryRegistry,
            IOccupantRegistry occupantRegistry,
            IPassportValidator passportValidator,
            IAuthManager authManager)
        {
            _currentEmployee = currentEmployee ?? throw new ArgumentNullException(nameof(currentEmployee));
            _permissionManager = permissionManager ?? throw new ArgumentNullException(nameof(permissionManager));
            _dormitoryRegistry = dormitoryRegistry ?? throw new ArgumentNullException(nameof(dormitoryRegistry));
            _occupantRegistry = occupantRegistry ?? throw new ArgumentNullException(nameof(occupantRegistry));
            _passportValidator = passportValidator ?? throw new ArgumentNullException(nameof(passportValidator));
            _authManager = authManager ?? throw new ArgumentNullException(nameof(authManager));
        }

        public void InitializeDashboard(DashboardView view)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));

            // Видимость кнопок по ролям
            _view.ManageDormitoriesButton.Visibility =
                _permissionManager.CanRolePerformAction(_currentEmployee.Role, "ManageDormitories")
                    ? Visibility.Visible : Visibility.Collapsed;

            _view.ManageRoomsButton.Visibility =
                _permissionManager.CanRolePerformAction(_currentEmployee.Role, "ManageRooms")
                    ? Visibility.Visible : Visibility.Collapsed;

            _view.ManageOccupantsButton.Visibility =
                _permissionManager.CanRolePerformAction(_currentEmployee.Role, "ManageOccupants")
                    ? Visibility.Visible : Visibility.Collapsed;

            _view.ManageDocumentsButton.Visibility =
                _permissionManager.CanRolePerformAction(_currentEmployee.Role, "ManageDocuments")
                    ? Visibility.Visible : Visibility.Collapsed;

            _view.ManageEvictionsButton.Visibility =
                _permissionManager.CanRolePerformAction(_currentEmployee.Role, "ManageEvictions")
                    ? Visibility.Visible : Visibility.Collapsed;

            _view.ManageInventoryButton.Visibility =
                _permissionManager.CanRolePerformAction(_currentEmployee.Role, "ManageInventory")
                    ? Visibility.Visible : Visibility.Collapsed;

            _view.ReportsButton.Visibility =
                _permissionManager.CanRolePerformAction(_currentEmployee.Role, "GenerateReports")
                    ? Visibility.Visible : Visibility.Collapsed;

            // Навигация: Справочник общежитий
            _view.ManageDormitoriesButton.Click += (s, e) =>
            {
                var dormitoriesView = new DormitoriesView();
                var presenter = new DormitoriesPresenter(
                    dormitoriesView,
                    _dormitoryRegistry,
                    _permissionManager,
                    _currentEmployee
                );
                dormitoriesView.Show();
            };

            // Навигация: Учёт комнат
            _view.ManageRoomsButton.Click += (s, e) =>
            {
                var roomsView = new RoomsView();
                var roomsPresenter = new RoomsPresenter(
                    roomsView,
                    _dormitoryRegistry,
                    _permissionManager,
                    _currentEmployee
                );
                roomsView.Show();
            };

            // Навигация: Регистрация жильцов и детей
            _view.ManageOccupantsButton.Click += (s, e) =>
            {
                var residentsView = new ResidentsView();
                var residentsPresenter = new ResidentsPresenter(
                    residentsView,
                    _occupantRegistry,
                    _permissionManager,
                    _passportValidator,
                    _currentEmployee
                );
                residentsView.Show();
            };

            // Logout
            _view.LogoutButton.Click += (s, e) =>
            {
                try
                {
                    _authManager.Logout();

                    var loginView = new LoginView();
                    // Назначаем новое главное окно до закрытия текущего, чтобы приложение не завершилось
                    Application.Current.MainWindow = loginView;

                    var loginPresenter = new LoginPresenter(_authManager, loginView, employee =>
                    {
                        // После повторного логина — обратно в Dashboard
                        var dashboardView = new DashboardView();
                        var dashboardPresenter = new DashboardPresenter(
                            employee,
                            _permissionManager,
                            _dormitoryRegistry,
                            _occupantRegistry,
                            _passportValidator,
                            _authManager
                        );
                        dashboardPresenter.InitializeDashboard(dashboardView);

                        Application.Current.MainWindow = dashboardView;
                        dashboardView.Show();
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