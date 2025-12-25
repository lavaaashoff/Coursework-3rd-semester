using CouseWork3Semester.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Models
{
    public class ValidationResult : IValidationResult
    {
        public bool IsValid { get; private set; }
        public List<string> Errors { get; private set; }
        public string Message { get; private set; }

        public ValidationResult()
        {
            IsValid = true;
            Errors = new List<string>();
            Message = "Validation successful";
        }

        public void AddError(string error)
        {
            Errors.Add(error);
            IsValid = false;
            Message = string.Join("; ", Errors);
        }
    }
}
