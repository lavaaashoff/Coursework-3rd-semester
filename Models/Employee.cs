using CouseWork3Semester.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Models
{
    public class Employee : IEmployee
    {
        // Свойства
        public string Login { get; private set; }
        public string Password { get; private set; }
        public string FullName { get; private set; }
        public string Role { get; private set; }

        // Конструктор
        public Employee(string login, string password, string fullName, string role)
        {
            Validate(login, password, fullName, role);

            Login = login;
            Password = password;
            FullName = fullName;
            Role = role;
        }

        // Реализация методов интерфейса
        public string GetData()
        {
            return $"Сотрудник: {FullName}, Логин: {Login}, Роль: {Role}";
        }

        public string GetRole()
        {
            return Role;
        }

        // Вспомогательные методы
        private void Validate(string login, string password, string fullName, string role)
        {
            if (string.IsNullOrWhiteSpace(login))
                throw new ArgumentException("Логин не может быть пустым", nameof(login));

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Пароль не может быть пустым", nameof(password));

            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException("ФИО не может быть пустым", nameof(fullName));

            if (string.IsNullOrWhiteSpace(role))
                throw new ArgumentException("Роль не может быть пустой", nameof(role));
        }
    }
}
