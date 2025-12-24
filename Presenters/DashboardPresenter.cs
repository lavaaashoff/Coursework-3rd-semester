using CourseWork3Semester.Views;
using CouseWork3Semester.Interfaces;
using System;

namespace CouseWork3Semester.Presenters
{
    public class DashboardPresenter
    {
        private readonly IEmployee _currentEmployee;
        private readonly IPermissionManager _permissionManager;

        public DashboardPresenter(IEmployee currentEmployee, IPermissionManager permissionManager)
        {
            _currentEmployee = currentEmployee ?? throw new ArgumentNullException(nameof(currentEmployee));
            _permissionManager = permissionManager ?? throw new ArgumentNullException(nameof(permissionManager));
        }

        public void InitializeDashboard(DashboardView view)
        {
            if (view == null) throw new ArgumentNullException(nameof(view));

            // Установка видимости кнопок на основе роли
            view.ManageDormitoriesButton.Visibility = _permissionManager.CanRolePerformAction(_currentEmployee.Role, "ManageDormitories")
                ? System.Windows.Visibility.Visible
                : System.Windows.Visibility.Collapsed;

            view.ManageRoomsButton.Visibility = _permissionManager.CanRolePerformAction(_currentEmployee.Role, "ManageRooms")
                ? System.Windows.Visibility.Visible
                : System.Windows.Visibility.Collapsed;

            view.ManageOccupantsButton.Visibility = _permissionManager.CanRolePerformAction(_currentEmployee.Role, "ManageOccupants")
                ? System.Windows.Visibility.Visible
                : System.Windows.Visibility.Collapsed;

            view.ManageDocumentsButton.Visibility = _permissionManager.CanRolePerformAction(_currentEmployee.Role, "ManageDocuments")
                ? System.Windows.Visibility.Visible
                : System.Windows.Visibility.Collapsed;

            view.ManageEvictionsButton.Visibility = _permissionManager.CanRolePerformAction(_currentEmployee.Role, "ManageEvictions")
                ? System.Windows.Visibility.Visible
                : System.Windows.Visibility.Collapsed;

            view.ManageInventoryButton.Visibility = _permissionManager.CanRolePerformAction(_currentEmployee.Role, "ManageInventory")
                ? System.Windows.Visibility.Visible
                : System.Windows.Visibility.Collapsed;

            view.ReportsButton.Visibility = _permissionManager.CanRolePerformAction(_currentEmployee.Role, "GenerateReports")
                ? System.Windows.Visibility.Visible
                : System.Windows.Visibility.Collapsed;
        }
    }
}