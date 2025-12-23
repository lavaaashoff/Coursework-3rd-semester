using CouseWork3Semester.Interfaces.IModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Models
{
    public class Document : IDocument
    {
        // Реализация свойств интерфейса
        public Guid Id { get; private set; }

        private string _series;
        public string Series
        {
            get => _series;
            private set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Document series cannot be null or empty", nameof(Series));

                if (value.Length < 2)
                    throw new ArgumentException("Document series is too short", nameof(Series));

                _series = value.Trim();
            }
        }

        private string _number;
        public string Number
        {
            get => _number;
            private set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Document number cannot be null or empty", nameof(Number));

                if (value.Length < 1)
                    throw new ArgumentException("Document number is too short", nameof(Number));

                _number = value.Trim();
            }
        }

        private string _title;
        public string Title
        {
            get => _title;
            private set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Document title cannot be null or empty", nameof(Title));

                if (value.Length < 3)
                    throw new ArgumentException("Document title is too short", nameof(Title));

                _title = value.Trim();
            }
        }

        private DateTime _issueDate;
        public DateTime IssueDate
        {
            get => _issueDate;
            private set
            {
                if (value > DateTime.Now)
                    throw new ArgumentException("Issue date cannot be in the future", nameof(IssueDate));

                if (value < DateTime.Now.AddYears(-100))
                    throw new ArgumentException("Issue date is too old", nameof(IssueDate));

                _issueDate = value.Date;
            }
        }

        private string _issuedBy;
        public string IssuedBy
        {
            get => _issuedBy;
            private set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Issuing authority cannot be null or empty", nameof(IssuedBy));

                if (value.Length < 3)
                    throw new ArgumentException("Issuing authority name is too short", nameof(IssuedBy));

                _issuedBy = value.Trim();
            }
        }

        private string _comment;
        public string Comment
        {
            get => _comment;
            private set => _comment = value?.Trim() ?? string.Empty;
        }

        // Конструкторы

        public Document(string series, string number, string title, DateTime issueDate, string issuedBy, string comment = null)
        {
            Id = Guid.NewGuid();
            Series = series;
            Number = number;
            Title = title;
            IssueDate = issueDate;
            IssuedBy = issuedBy;
            Comment = comment;
        }

        // Конструктор для загрузки из базы данных (с существующим Id)
        public Document(Guid id, string series, string number, string title, DateTime issueDate, string issuedBy, string comment)
            : this(series, number, title, issueDate, issuedBy, comment)
        {
            Id = id;
        }

        // Реализация методов интерфейса
        public void ChangeComment(string text)
        {
            Comment = text;
        }

        public string GetFullData()
        {
            return $"Document: {Title}\n" +
                   $"ID: {Id}\n" +
                   $"Series: {Series}\n" +
                   $"Number: {Number}\n" +
                   $"Issue Date: {IssueDate:dd.MM.yyyy}\n" +
                   $"Issued By: {IssuedBy}\n" +
                   $"Comment: {Comment}\n";
        }

        // Дополнительные методы



        public override bool Equals(object obj)
        {
            if (obj is Document other)
            {
                return Series == other.Series &&
                       Number == other.Number &&
                       Title == other.Title;
            }
            return false;
        }
    }
}
