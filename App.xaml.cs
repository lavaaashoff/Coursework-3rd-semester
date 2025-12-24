using CouseWork3Semester.Interfaces;
using CouseWork3Semester.Models;
using CouseWork3Semester.Presenters;
using CouseWork3Semester.Services;
using CouseWork3Semester.Views;
using CouseWork3Semester.Enums;
using System.Windows;

namespace CouseWork3Semester
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Список сотрудников (пример данных) ПОТОМ НАДО УДАЛИТЬ!!!!!!!!!!!!!!!!!!!!!!!!!!
            var employees = new List<IEmployee> { new Employee("lvshf", "123", "Vital Suhomlinvo", UserRole.Administrator)};
            var authManager = new AuthManager(employees);

            var loginView = new LoginView();
            var loginPresenter = new LoginPresenter(authManager, loginView);

            loginView.Show();
        }
    }
}