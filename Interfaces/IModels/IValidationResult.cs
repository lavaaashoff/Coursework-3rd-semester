using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Interfaces.IModels
{
    public interface IValidationResult
    {
        bool IsValid { get; }
        List<string> Errors { get; }
        string Message { get; }

        void AddError(string error);

    }
}
