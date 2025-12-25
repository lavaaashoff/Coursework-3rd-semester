using CouseWork3Semester.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CouseWork3Semester.Models
{
    public class DocumentValidator : IDocumentValidator
    {
        private const int DOCUMENT_SERIES_LENGTH = 4;
        private const int DOCUMENT_NUMBER_LENGTH = 6;
        private const int DOCUMENT_TITLE_MIN_LENGTH = 3;
        private const int MIN_ISSUING_AUTHORITY_LENGTH = 3;
        private const int DOCUMENT_VALIDITY_YEARS = 10;

        private readonly int _maxCommentLength = 1000;

        public IValidationResult CheckFormat(IDocument document)
        {
            var result = new ValidationResult();

            if (document == null)
            {
                result.AddError("Документ не может быть null");
                return result;
            }

            if (string.IsNullOrWhiteSpace(document.Series))
            {
                result.AddError("Серия документа не может быть пустой");
            }
            else if (document.Series.Length != DOCUMENT_SERIES_LENGTH)
            {
                result.AddError($"Серия документа должна быть длинной {DOCUMENT_SERIES_LENGTH} символов");
            }

            if (string.IsNullOrWhiteSpace(document.Number))
            {
                result.AddError("Номер документа не может быть пустым");
            }
            else if (document.Number.Length != DOCUMENT_NUMBER_LENGTH)
            {
                result.AddError($"Document number must be {DOCUMENT_NUMBER_LENGTH} characters");
            }
            else if (!Regex.IsMatch(document.Number, @"^[0-9]{6}$"))
            {
                result.AddError("Document number must contain only digits");
            }

            if (string.IsNullOrWhiteSpace(document.Title))
            {
                result.AddError("Название документа не может быть пустым");
            }
            else if (document.Title.Length < DOCUMENT_TITLE_MIN_LENGTH)
            {
                result.AddError($"Название документа должно содержать минимум {DOCUMENT_TITLE_MIN_LENGTH} символа");
            }

            if (document.IssueDate == default)
            {
                result.AddError("Issue date is not set");
            }
            else if (document.IssueDate > DateTime.Now)
            {
                result.AddError("Issue date cannot be in the future");
            }

            if (string.IsNullOrWhiteSpace(document.IssuedBy))
            {
                result.AddError("Issuing authority is empty");
            }
            else if (document.IssuedBy.Length < MIN_ISSUING_AUTHORITY_LENGTH)
            {
                result.AddError($"Issuing authority name is too short (minimum {MIN_ISSUING_AUTHORITY_LENGTH} characters)");
            }

            if (!string.IsNullOrEmpty(document.Comment) && document.Comment.Length > _maxCommentLength)
            {
                result.AddError($"Комментарий не должен превышать {_maxCommentLength} символов");
            }

            return result;
        }

        public bool CheckValidity(IDocument document, DateTime checkDate)
        {
            if (document == null)
                return false;

            var formatCheck = CheckFormat(document);
            if (!formatCheck.IsValid)
                return false;

            if (document.IssueDate > checkDate)
                return false;

            DateTime dateToCheck = checkDate != DateTime.MinValue ? checkDate : DateTime.Now;

            if (document.IssueDate.AddYears(DOCUMENT_VALIDITY_YEARS) < checkDate)
                return false;

            return true;
        }

        public bool CheckUniqueness(IDocument document, IEnumerable<IDocument> existingDocuments)
        {
            if (document == null || existingDocuments == null)
                return false;

            foreach (var existing in existingDocuments)
            {
                if (existing == null)
                    continue;

                if (existing.Series == document.Series && existing.Number == document.Number)
                {
                    if (existing.IssueDate != document.IssueDate)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
