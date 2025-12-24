using CourseWork3Semester.Views;
using CouseWork3Semester.Interfaces;
using CouseWork3Semester.Views;
using System;
using System.Windows;

namespace CouseWork3Semester.Presenters
{
    public class DashboardPresenter
    {
        private readonly IEmployee _currentEmployee;
        private readonly IPermissionManager _permissionManager;
        private readonly IDormitoryRegistry _dormitoryRegistry;

        public DashboardPresenter(
            IEmployee currentEmployee,
            IPermissionManager permissionManager,
            IDormitoryRegistry dormitoryRegistry)
        {
            _currentEmployee = currentEmployee ?? throw new ArgumentNullException(nameof(currentEmployee));
            _permissionManager = permissionManager ?? throw new ArgumentNullException(nameof(permissionManager));
            _dormitoryRegistry = dormitoryRegistry ?? throw new ArgumentNullException(nameof(dormitoryRegistry));
        }

        public void InitializeDashboard(DashboardView view)
        {
            if (view == null) throw new ArgumentNullException(nameof(view));

            // Видимость кнопок по ролям
            view.ManageDormitoriesButton.Visibility =
                _permissionManager.CanRolePerformAction(_currentEmployee.Role, "ManageDormitories")
                    ? Visibility.Visible : Visibility.Collapsed;

            view.ManageRoomsButton.Visibility =
                _permissionManager.CanRolePerformAction(_currentEmployee.Role, "ManageRooms")
                    ? Visibility.Visible : Visibility.Collapsed;

            view.ManageOccupantsButton.Visibility =
                _permissionManager.CanRolePerformAction(_currentEmployee.Role, "ManageOccupants")
                    ? Visibility.Visible : Visibility.Collapsed;

            view.ManageDocumentsButton.Visibility =
                _permissionManager.CanRolePerformAction(_currentEmployee.Role, "ManageDocuments")
                    ? Visibility.Visible : Visibility.Collapsed;

            view.ManageEvictionsButton.Visibility =
                _permissionManager.CanRolePerformAction(_currentEmployee.Role, "ManageEvictions")
                    ? Visibility.Visible : Visibility.Collapsed;

            view.ManageInventoryButton.Visibility =
                _permissionManager.CanRolePerformAction(_currentEmployee.Role, "ManageInventory")
                    ? Visibility.Visible : Visibility.Collapsed;

            view.ReportsButton.Visibility =
                _permissionManager.CanRolePerformAction(_currentEmployee.Role, "GenerateReports")
                    ? Visibility.Visible : Visibility.Collapsed;

            // ОБРАБОТЧИК КНОПКИ «Справочник общежитий» — вот сюда и нужно добавить
            view.ManageDormitoriesButton.Click += (s, e) =>
            {
                // Создаём и открываем окно справочника общежитий
                var dormitoriesView = new DormitoriesView();
                var presenter = new DormitoriesPresenter(
                    dormitoriesView,
                    _dormitoryRegistry,
                    _permissionManager,
                    _currentEmployee
                );

                dormitoriesView.Show();
            };

            view.ManageRoomsButton.Click += (s, e) =>
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
        }
    }
}