using System.Collections.Generic;
using System.Windows;
using CouseWork3Semester.Enums;
using CouseWork3Semester.Interfaces;
using CouseWork3Semester.Models;
using CouseWork3Semester.Presenters;
using CouseWork3Semester.Registries;
using CouseWork3Semester.Services;
using CouseWork3Semester.Views;

namespace CouseWork3Semester
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Current.ShutdownMode = ShutdownMode.OnMainWindowClose;

            // Пример данных (удалить позже)
            var employees = new List<IEmployee>
            {
                new Employee("admin", "admin", "Vital Suhomlinvo", UserRole.Administrator),
                new Employee("com", "com", "Vital Suhomlinvo", UserRole.Commandant),
                new Employee("staff", "staff", "Vital Suhomlinvo", UserRole.AdminStaff)
            };
            var authManager = new AuthManager(employees);

            // Глобальные зависимости, которые будем передавать в Dashboard
            var permissionManager = new PermissionManager();
            var dormitoryRegistry = new DormitoryRegistry();
            var occupantRegistry = new OccupantRegistry();
            var passportValidator = new PassportValidator();

            // Открытие Login
            var loginView = new LoginView();
            Application.Current.MainWindow = loginView;

            var loginPresenter = new LoginPresenter(authManager, loginView, currentEmployee =>
            {
                var dashboardView = new DashboardView();
                var dashboardPresenter = new DashboardPresenter(
                    currentEmployee,
                    permissionManager,
                    dormitoryRegistry,
                    occupantRegistry,
                    passportValidator,
                    authManager
                );

                dashboardPresenter.InitializeDashboard(dashboardView);
                Application.Current.MainWindow = dashboardView;
                dashboardView.Show();
            });

            loginView.Show();
        }
    }
}