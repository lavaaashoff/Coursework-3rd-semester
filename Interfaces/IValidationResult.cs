using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Interfaces
{
    public interface IValidationResult
    {
        bool IsValid { get; }
        List<string> Errors { get; }
        string Message { get; }
    }
}
