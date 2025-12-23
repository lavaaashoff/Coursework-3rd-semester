using CouseWork3Semester.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Models
{
    public class Passport : IPassport
    {
        // Приватные поля для валидации
        private string _series;
        private string _number;
        private DateTime _issueDate;
        private string _issuedBy;

        // Свойства интерфейса
        public string Series
        {
            get => _series;
            private set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Passport series cannot be null or empty", nameof(Series));

                if (value.Length != 4)
                    throw new ArgumentException("Passport series must be 4 characters", nameof(Series));

                _series = value;
            }
        }

        public string Number
        {
            get => _number;
            private set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Passport number cannot be null or empty", nameof(Number));

                if (value.Length != 6)
                    throw new ArgumentException("Passport number must be 6 characters", nameof(Number));

                _number = value;
            }
        }

        public DateTime IssueDate
        {
            get => _issueDate;
            private set
            {
                // Проверка, что дата не в будущем
                if (value > DateTime.Now)
                    throw new ArgumentException("Issue date cannot be in the future", nameof(IssueDate));

                // Проверка, что дата не слишком старая (например, не старше 100 лет)
                if (value < DateTime.Now.AddYears(-100))
                    throw new ArgumentException("Issue date is too old", nameof(IssueDate));

                _issueDate = value;
            }
        }

        public string IssuedBy
        {
            get => _issuedBy;
            private set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Issuing authority cannot be null or empty", nameof(IssuedBy));

                if (value.Length < 5)
                    throw new ArgumentException("Issuing authority name is too short", nameof(IssuedBy));

                _issuedBy = value;
            }
        }

        // Конструкторы
        public Passport(string series, string number, DateTime issueDate, string issuedBy)
        {
            Series = series;
            Number = number;
            IssueDate = issueDate;
            IssuedBy = issuedBy;
        }

        // Конструктор для создания паспорта с текущей датой выдачи
        public Passport(string series, string number, string issuedBy)
            : this(series, number, DateTime.Now, issuedBy)
        {
        }

        // Методы интерфейса

        public string GetFullPassportData()
        {
            return $"Passport: {Series} {Number}\n" +
                   $"Issued: {IssueDate:dd.MM.yyyy}\n" +
                   $"Issued by: {IssuedBy}\n" +
                   $"Years since issue: {GetYearsSinceIssue()}";
        }

        // Дополнительные методы

        public string GetPassportSeriesAndNumber()
        {
            return $"{Series} {Number}";
        }

        public int GetYearsSinceIssue()
        {
            DateTime today = DateTime.Today;
            int years = today.Year - IssueDate.Year;

            // Если день выдачи еще не наступил в этом году, вычитаем 1 год
            if (IssueDate.Date > today.AddYears(-years))
            {
                years--;
            }

            return years;
        }

        public bool IsPassportExpired(int expirationYears = 10)
        {
            return GetYearsSinceIssue() > expirationYears;
        }

        // Переопределение стандартных методов

        public override string ToString()
        {
            return GetPassportSeriesAndNumber();
        }

        public override bool Equals(object obj)
        {
            if (obj is Passport other)
            {
                return Series == other.Series &&
                       Number == other.Number &&
                       IssueDate == other.IssueDate;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Series, Number, IssueDate);
        }
    }
}
