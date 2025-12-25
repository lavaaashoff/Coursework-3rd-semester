using System.Collections.Generic;
using System.IO;
using System;
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
            DebugLogger.Write("App.OnStartup started");

            // Не завершаем процесс при закрытии Login
            Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            var employees = new List<IEmployee>
            {
                new Employee("adm", "adm", "Vital Suhomlinvo", UserRole.Administrator),
                new Employee("com", "com", "Vital Suhomlinvo", UserRole.Administrator),
                new Employee("adm", "com", "Vital Suhomlinvo", UserRole.Administrator)
            };
            var authManager = new AuthManager(employees);
            var permissionManager = new PermissionManager();

            var storagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Storage", "state.json");
            var storage = new JsonStorageService(storagePath);

            var passportValidator = new PassportValidator();
            var documentValidator = new DocumentValidator();

            var loginView = new LoginView();
            DebugLogger.Write("Showing LoginView");
            var loginPresenter = new LoginPresenter(authManager, loginView, employee =>
            {
                DebugLogger.Write($"Login success for user={employee?.Login}");

                DormitoryRegistry dormitoryRegistry;
                OccupantRegistry occupantRegistry;
                DocumentRegistry documentRegistry;
                SettlementEvictionService settlementEvictionService;
                InventoryRegistry inventoryRegistry;

                if (storage.TryLoad(out var state) && state != null)
                {
                    DebugLogger.Write("State loaded, building registries from state");
                    dormitoryRegistry = state.DormitoryRegistry ?? new DormitoryRegistry();
                    occupantRegistry = state.OccupantRegistry ?? new OccupantRegistry();
                    documentRegistry = state.DocumentRegistry ?? new DocumentRegistry();
                    settlementEvictionService = state.SettlementEvictionService ?? new SettlementEvictionService();
                    inventoryRegistry = state.InventoryRegistry ?? new InventoryRegistry();
                }
                else
                {
                    DebugLogger.Write("No state available, building empty registries");
                    dormitoryRegistry = new DormitoryRegistry();
                    occupantRegistry = new OccupantRegistry();
                    documentRegistry = new DocumentRegistry();
                    settlementEvictionService = new SettlementEvictionService();
                    inventoryRegistry = new InventoryRegistry();
                }

                var reportService = new ReportService(dormitoryRegistry, occupantRegistry);
                var searchService = new SearchService(dormitoryRegistry, occupantRegistry);
                var documentOccupantService = new DocumentOccupantService(documentRegistry, occupantRegistry, documentValidator);

                var sysForUser = new AccountingSystem(
                    dormitoryRegistry,
                    occupantRegistry,
                    settlementEvictionService: settlementEvictionService,
                    reportService: reportService,
                    searchService: searchService,
                    authManager,
                    permissionManager,
                    documentOccupantService,
                    passportValidator,
                    documentValidator,
                    currentEmployee: employee,
                    documentRegistry: documentRegistry,
                    inventoryRegistry: inventoryRegistry
                );

                DebugLogger.Write("AccountingSystem created, preparing DashboardView");

                var dashboardView = new DashboardView();
                var dashboardPresenter = new DashboardPresenter(sysForUser, storage);
                dashboardPresenter.InitializeDashboard(dashboardView);

                Application.Current.MainWindow = dashboardView;
                Current.ShutdownMode = ShutdownMode.OnMainWindowClose;

                try
                {
                    dashboardView.Show();
                    dashboardView.Activate();
                    DebugLogger.Write("DashboardView shown and activated");
                    loginView.Close();
                    DebugLogger.Write("LoginView closed");
                }
                catch (Exception exShow)
                {
                    DebugLogger.Write("Failed to show DashboardView", exShow);
                    // Вернём Login обратно, чтобы не висеть без окна
                    loginView.Show();
                    MessageBox.Show($"Failed to open dashboard: {exShow.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

            loginView.Show();
        }
    }
}