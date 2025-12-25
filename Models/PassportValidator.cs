using CouseWork3Semester.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace CouseWork3Semester.Models
{
    public class PassportValidator : IPassportValidator
    {
        private const int PASSPORT_SERIES_LENGTH = 4;
        private const int PASSPORT_NUMBER_LENGTH = 6;
        private const int MIN_ISSUING_AUTHORITY_LENGTH = 3;

        private const int PASSPORT_VALIDITY_YEARS = 10;

        public IValidationResult CheckFormat(IPassport passport)
        {
            var result = new ValidationResult();

            if (passport == null)
            {
                result.AddError("Passport is null");
                return result;
            }

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

            if (passport.IssueDate == default)
            {
                result.AddError("Issue date is not set");
            }
            else if (passport.IssueDate > DateTime.Now)
            {
                result.AddError("Issue date cannot be in the future");
            }

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

            var formatCheck = CheckFormat(passport);
            if (!formatCheck.IsValid)
                return false;

            if (passport.IssueDate > onDate)
                return false;

            if (passport.IssueDate.AddYears(PASSPORT_VALIDITY_YEARS) < onDate)
                return false; 

            return true;
        }

        public bool CheckUniqueness(IPassport passport, List<IPassport> existingPassports)
        {
            if (passport == null || existingPassports == null)
                return false;

            foreach (var existing in existingPassports)
            {
                if (existing == null)
                    continue;

                if (existing.Series == passport.Series && existing.Number == passport.Number)
                {
                    if (existing.IssueDate != passport.IssueDate)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
