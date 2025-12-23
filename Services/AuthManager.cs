using CouseWork3Semester.Interfaces;
using CouseWork3Semester.Interfaces.IModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Services
{
    public class AuthManager : IAuthManager
    {
        // Список сотрудников (в реальном приложении это было бы в базе данных)
        private readonly List<IEmployee> employees;
        private IEmployee currentUser;

        // Конструктор
        public AuthManager(List<IEmployee> employeesList)
        {
            employees = employeesList ?? throw new ArgumentNullException(nameof(employeesList));
            currentUser = null;
        }

        // Реализация методов интерфейса

        public void Login(string login, string password)
        {
            if (string.IsNullOrWhiteSpace(login))
                throw new ArgumentException("Логин не может быть пустым", nameof(login));

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Пароль не может быть пустым", nameof(password));

            // Поиск сотрудника по логину
            var employee = employees.FirstOrDefault(e => e.Login == login);

            if (employee == null)
                throw new UnauthorizedAccessException("Пользователь не найден");

            // Проверка пароля
            if (employee.Password != password)
                throw new UnauthorizedAccessException("Неверный пароль");

            currentUser = employee;
            Console.WriteLine($"Вход выполнен: {employee.FullName}, Роль: {employee.Role}");
        }

        public void ChangePassword(IEmployee employee, string oldPassword, string newPassword)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));

            if (string.IsNullOrWhiteSpace(oldPassword))
                throw new ArgumentException("Старый пароль не может быть пустым", nameof(oldPassword));

            if (string.IsNullOrWhiteSpace(newPassword))
                throw new ArgumentException("Новый пароль не может быть пустым", nameof(newPassword));

            // Проверяем, что старый пароль верный
            if (employee.Password != oldPassword)
                throw new UnauthorizedAccessException("Неверный старый пароль");

            // Проверяем, что новый пароль отличается от старого
            if (oldPassword == newPassword)
                throw new ArgumentException("Новый пароль должен отличаться от старого");

            // Меняем пароль (в реальном приложении это было бы через метод Employee)
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

        // Дополнительные методы

        public IEmployee GetCurrentUser()
        {
            return currentUser;
        }

        public bool IsLoggedIn()
        {
            return currentUser != null;
        }

        public bool HasRole(string requiredRole)
        {
            if (currentUser == null || string.IsNullOrEmpty(requiredRole))
                return false;

            return currentUser.Role == requiredRole;
        }

        public void RegisterEmployee(IEmployee newEmployee)
        {
            if (newEmployee == null)
                throw new ArgumentNullException(nameof(newEmployee));

            // Проверяем, нет ли уже сотрудника с таким логином
            if (employees.Any(e => e.Login == newEmployee.Login))
                throw new InvalidOperationException($"Сотрудник с логином '{newEmployee.Login}' уже существует");

            employees.Add(newEmployee);
            Console.WriteLine($"Зарегистрирован новый сотрудник: {newEmployee.FullName}");
        }

        public List<IEmployee> GetAllEmployees()
        {
            return new List<IEmployee>(employees);
        }

        public override string ToString()
        {
            return $"Auth Manager: {employees.Count} сотрудников, " +
                   $"Текущий пользователь: {(currentUser?.FullName ?? "не авторизован")}";
        }
    }
}
