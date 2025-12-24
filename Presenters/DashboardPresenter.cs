using CouseWork3Semester.Views;
using CouseWork3Semester.Interfaces;
using System;
using System.Windows;

namespace CouseWork3Semester.Presenters
{
    public class DashboardPresenter
    {
        private readonly IEmployee _currentEmployee;
        private readonly IPermissionManager _permissionManager;
        private readonly IDormitoryRegistry _dormitoryRegistry;
        private readonly IAuthManager _authManager;

        private DashboardView _view;

        public DashboardPresenter(
            IEmployee currentEmployee,
            IPermissionManager permissionManager,
            IDormitoryRegistry dormitoryRegistry,
            IAuthManager authManager)
        {
            _currentEmployee = currentEmployee ?? throw new ArgumentNullException(nameof(currentEmployee));
            _permissionManager = permissionManager ?? throw new ArgumentNullException(nameof(permissionManager));
            _dormitoryRegistry = dormitoryRegistry ?? throw new ArgumentNullException(nameof(dormitoryRegistry));
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

            // Открытие окна "Справочник общежитий"
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

            // Открытие окна "Учёт комнат"
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

            // Logout
            _view.LogoutButton.Click += (s, e) =>
            {
                try
                {
                    _authManager.Logout();

                    // Переход обратно на Login
                    var loginView = new LoginView();

                    // Важно: назначаем новое главное окно до закрытия Dashboard
                    Application.Current.MainWindow = loginView;

                    var loginPresenter = new LoginPresenter(_authManager, loginView, employee =>
                    {
                        // После повторного логина — обратно в Dashboard
                        var permissionManager = _permissionManager; // можно переиспользовать
                        var dormitoryRegistry = _dormitoryRegistry; // переиспользуем общий реестр

                        var dashboardView = new DashboardView();
                        var dashboardPresenter = new DashboardPresenter(employee, permissionManager, dormitoryRegistry, _authManager);
                        dashboardPresenter.InitializeDashboard(dashboardView);

                        Application.Current.MainWindow = dashboardView;
                        dashboardView.Show();
                    });

                    loginView.Show();

                    // Закрываем текущий Dashboard
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