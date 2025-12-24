using CouseWork3Semester.Interfaces;
using CouseWork3Semester.Models;
using CouseWork3Semester.Presenters;
using CouseWork3Semester.Services;
using CouseWork3Semester.Views;
using CourseWork3Semester.Views;
using CouseWork3Semester.Enums;
using System.Windows;
using System.Collections.Generic;

namespace CouseWork3Semester
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Критично: управляем режимом завершения
            Current.ShutdownMode = ShutdownMode.OnMainWindowClose;

            // Список сотрудников (пример данных) ПОТОМ НАДО УДАЛИТЬ!!!!!!!!!!!!!!!!!!!!!!!!!!
            var employees = new List<IEmployee>
            {
                new Employee("lvshf", "123", "Vital Suhomlinvo", UserRole.Administrator)
            };
            var authManager = new AuthManager(employees);

            // Открытие LoginView с передачей AuthManager и обработчиком после логина
            var loginView = new LoginView();
            var loginPresenter = new LoginPresenter(authManager, loginView, currentEmployee =>
            {
                // 1) Создаём Dashboard
                var permissionManager = new PermissionManager();
                var dashboardView = new DashboardView();
                var dashboardPresenter = new DashboardPresenter(currentEmployee, permissionManager);
                dashboardPresenter.InitializeDashboard(dashboardView);

                // 2) Делаем Dashboard главным окном до закрытия Login
                Application.Current.MainWindow = dashboardView;

                // 3) Показываем Dashboard
                dashboardView.Show();
            });

            // Login будет главным окном, пока не назначим Dashboard
            Application.Current.MainWindow = loginView;
            loginView.Show();
        }
    }
}