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

            var employees = new List<IEmployee>
            {
                new Employee("adm", "adm", "Vital Suhomlinvo", UserRole.Administrator),
                new Employee("com", "com", "Vital Suhomlinvo", UserRole.Administrator),
                new Employee("adm", "com", "Vital Suhomlinvo", UserRole.Administrator)
            };
            var authManager = new AuthManager(employees);

            // Создаём зависимости
            var permissionManager = new PermissionManager();
            var dormitoryRegistry = new DormitoryRegistry();
            var occupantRegistry = new OccupantRegistry();
            var passportValidator = new PassportValidator();
            var documentRegistry = new DocumentRegistry();
            var documentValidator = new DocumentValidator();
            var documentOccupantService = new DocumentOccupantService(documentRegistry, occupantRegistry, documentValidator);

            // Новый реестр инвентаря
            var inventoryRegistry = new InventoryRegistry();

            // Собираем AccountingSystem (без текущего сотрудника)
            var accountingSystem = new AccountingSystem(
                dormitoryRegistry,
                occupantRegistry,
                settlementEvictionService: new SettlementEvictionService(),
                paymentService: new PaymentService(),
                reportService: new ReportService(dormitoryRegistry, occupantRegistry, new PaymentService()),
                searchService: new SearchService(dormitoryRegistry, occupantRegistry),
                authManager,
                permissionManager,
                documentOccupantService,
                passportValidator,
                documentValidator,
                currentEmployee: null,
                documentRegistry: documentRegistry,
                inventoryRegistry: inventoryRegistry
            );

            // Login
            var loginView = new LoginView();
            Application.Current.MainWindow = loginView;

            var loginPresenter = new LoginPresenter(authManager, loginView, employee =>
            {
                // Пересобираем систему с текущим сотрудником
                var sysForUser = new AccountingSystem(
                    accountingSystem.DormitoryRegistry,
                    accountingSystem.OccupantRegistry,
                    accountingSystem.SettlementEvictionService,
                    accountingSystem.PaymentService,
                    accountingSystem.ReportService,
                    accountingSystem.SearchService,
                    accountingSystem.AuthManager,
                    accountingSystem.PermissionManager,
                    accountingSystem.DocumentOccupantService,
                    accountingSystem.PassportValidator,
                    accountingSystem.DocumentValidator,
                    currentEmployee: employee,
                    documentRegistry: accountingSystem.DocumentRegistry,
                    inventoryRegistry: accountingSystem.InventoryRegistry
                );

                var dashboardView = new DashboardView();
                var dashboardPresenter = new DashboardPresenter(sysForUser);
                dashboardPresenter.InitializeDashboard(dashboardView);
                Application.Current.MainWindow = dashboardView;
                dashboardView.Show();
            });

            loginView.Show();
        }
    }
}