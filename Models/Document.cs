using CouseWork3Semester.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CouseWork3Semester.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Document : IDocument
    {
        [JsonProperty]
        public Guid Id { get; private set; }

        private string _series;
        [JsonProperty]
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
        [JsonProperty]
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
        [JsonProperty]
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
        [JsonProperty]
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
        [JsonProperty]
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

        [JsonProperty]
        public string Comment { get; set; }

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

        [JsonConstructor]
        public Document(Guid id, string series, string number, string title, DateTime issueDate, string issuedBy, string comment = null)
            : this(series, number, title, issueDate, issuedBy, comment)
        {
            Id = id;
        }

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
    }
}