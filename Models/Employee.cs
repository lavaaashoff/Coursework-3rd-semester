using CouseWork3Semester.Interfaces;
using CouseWork3Semester.Enums;
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
        public UserRole Role { get; private set; }

        // Конструктор
        public Employee(string login, string password, string fullName, UserRole role)
        {
            Validate(login, password, fullName);

            Login = login;
            Password = password;
            FullName = fullName;
            Role = role;
        }

        // Реализация методов интерфейса

        public void ChangePassword(string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
                throw new ArgumentException("Новый пароль не может быть пустым", nameof(newPassword));

            Password = newPassword;
        }

        // Вспомогательные методы
        private void Validate(string login, string password, string fullName)
        {
            if (string.IsNullOrWhiteSpace(login))
                throw new ArgumentException("Логин не может быть пустым", nameof(login));

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Пароль не может быть пустым", nameof(password));

            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException("ФИО не может быть пустым", nameof(fullName));
        }
    }
}
