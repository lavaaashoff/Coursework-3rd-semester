using CouseWork3Semester.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Models
{
    public class Passport : IPassport
    {
        private string _series;
        private string _number;
        private DateTime _issueDate;
        private string _issuedBy;

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
                if (value > DateTime.Now)
                    throw new ArgumentException("Issue date cannot be in the future", nameof(IssueDate));

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

        public Passport(string series, string number, DateTime issueDate, string issuedBy)
        {
            Series = series;
            Number = number;
            IssueDate = issueDate;
            IssuedBy = issuedBy;
        }

        public Passport(string series, string number, string issuedBy)
            : this(series, number, DateTime.Now, issuedBy)
        {
        }


        public string GetFullPassportData()
        {
            return $"Passport: {Series} {Number}\n" +
                   $"Issued: {IssueDate:dd.MM.yyyy}\n" +
                   $"Issued by: {IssuedBy}\n" +
                   $"Years since issue: {GetYearsSinceIssue()}";
        }


        public int GetYearsSinceIssue()
        {
            DateTime today = DateTime.Today;
            int years = today.Year - IssueDate.Year;

            if (IssueDate.Date > today.AddYears(-years))
            {
                years--;
            }

            return years;
        }
    }
}
