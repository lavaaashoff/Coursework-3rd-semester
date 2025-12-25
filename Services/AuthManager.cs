using CouseWork3Semester.Enums;
using CouseWork3Semester.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Services
{
    public class AuthManager : IAuthManager
    {
        private readonly List<IEmployee> employees;
        private IEmployee currentUser;

        public AuthManager(List<IEmployee> employeesList)
        {
            employees = employeesList ?? throw new ArgumentNullException(nameof(employeesList));
            currentUser = null;
        }


        public IEmployee Login(string login, string password)
        {
            if (string.IsNullOrWhiteSpace(login))
                throw new ArgumentException("Логин не может быть пустым", nameof(login));

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Пароль не может быть пустым", nameof(password));

            var employee = employees.FirstOrDefault(e => e.Login == login);

            if (employee == null)
                throw new UnauthorizedAccessException("Пользователь не найден");

            if (employee.Password != password)
                throw new UnauthorizedAccessException("Неверный пароль");

            currentUser = employee;
            Console.WriteLine($"Вход выполнен: {employee.FullName}, Роль: {employee.Role}");
            return currentUser;
        }

        public void ChangePassword(IEmployee employee, string oldPassword, string newPassword)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));

            if (string.IsNullOrWhiteSpace(oldPassword))
                throw new ArgumentException("Старый пароль не может быть пустым", nameof(oldPassword));

            if (string.IsNullOrWhiteSpace(newPassword))
                throw new ArgumentException("Новый пароль не может быть пустым", nameof(newPassword));

            if (employee.Password != oldPassword)
                throw new UnauthorizedAccessException("Неверный старый пароль");

            if (oldPassword == newPassword)
                throw new ArgumentException("Новый пароль должен отличаться от старого");

            if (employee is Models.Employee concreteEmployee)
            {
                concreteEmployee.ChangePassword(newPassword);
                Console.WriteLine($"Пароль для {employee.FullName} успешно изменен");
            }
            else
            {
                throw new InvalidOperationException("Не удалось изменить пароль для данного типа сотрудника");
            }
        }

        public void Logout()
        {
            if (currentUser != null)
            {
                Console.WriteLine($"Выход выполнен: {currentUser.FullName}");
                currentUser = null;
            }
        }
    }
}
