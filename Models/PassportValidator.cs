using CouseWork3Semester.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace CouseWork3Semester.Models
{
    public class PassportValidator : IPassportValidator
    {
        // Константы для проверки формата
        private const int PASSPORT_SERIES_LENGTH = 4;
        private const int PASSPORT_NUMBER_LENGTH = 6;
        private const int MIN_ISSUING_AUTHORITY_LENGTH = 3;

        // Время действительности паспорта в годах
        private const int PASSPORT_VALIDITY_YEARS = 10;

        public IValidationResult CheckFormat(IPassport passport)
        {
            var result = new ValidationResult();

            if (passport == null)
            {
                result.AddError("Passport is null");
                return result;
            }

            // Проверка серии
            if (string.IsNullOrWhiteSpace(passport.Series))
            {
                result.AddError("Passport series is empty");
            }
            else if (passport.Series.Length != PASSPORT_SERIES_LENGTH)
            {
                result.AddError($"Passport series must be {PASSPORT_SERIES_LENGTH} characters");
            }
            else if (!Regex.IsMatch(passport.Series, @"^[0-9]{4}$"))
            {
                result.AddError("Passport series must contain only digits");
            }

            // Проверка номера
            if (string.IsNullOrWhiteSpace(passport.Number))
            {
                result.AddError("Passport number is empty");
            }
            else if (passport.Number.Length != PASSPORT_NUMBER_LENGTH)
            {
                result.AddError($"Passport number must be {PASSPORT_NUMBER_LENGTH} characters");
            }
            else if (!Regex.IsMatch(passport.Number, @"^[0-9]{6}$"))
            {
                result.AddError("Passport number must contain only digits");
            }

            // Проверка даты выдачи
            if (passport.IssueDate == default)
            {
                result.AddError("Issue date is not set");
            }
            else if (passport.IssueDate > DateTime.Now)
            {
                result.AddError("Issue date cannot be in the future");
            }

            // Проверка органа выдачи
            if (string.IsNullOrWhiteSpace(passport.IssuedBy))
            {
                result.AddError("Issuing authority is empty");
            }
            else if (passport.IssuedBy.Length < MIN_ISSUING_AUTHORITY_LENGTH)
            {
                result.AddError($"Issuing authority name is too short (minimum {MIN_ISSUING_AUTHORITY_LENGTH} characters)");
            }
            return result;
        }

        public bool CheckValidity(IPassport passport, DateTime onDate)
        {
            if (passport == null)
                return false;

            // Проверяем формат
            var formatCheck = CheckFormat(passport);
            if (!formatCheck.IsValid)
                return false;

            // Проверяем, что дата выдачи не в будущем относительно onDate
            if (passport.IssueDate > onDate)
                return false;

            // Проверяем срок действия (если есть)
            if (passport.IssueDate.AddYears(PASSPORT_VALIDITY_YEARS) < onDate)
                return false; // Паспорт слишком старый

            return true;
        }

        public bool CheckUniqueness(IPassport passport, List<IPassport> existingPassports)
        {
            if (passport == null || existingPassports == null)
                return false;

            // Проверяем, нет ли такого же паспорта в списке
            foreach (var existing in existingPassports)
            {
                if (existing == null)
                    continue;

                // Паспорт считается одинаковым, если совпадают серия и номер
                if (existing.Series == passport.Series && existing.Number == passport.Number)
                {
                    // Но это может быть один и тот же паспорт (проверяем по дате выдачи)
                    if (existing.IssueDate != passport.IssueDate)
                    {
                        return false; // Найден другой паспорт с такими же данными
                    }
                }
            }

            return true;
        }
    }
}
