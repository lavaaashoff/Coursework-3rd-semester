using CouseWork3Semester.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Interfaces
{
    public interface IPassportValidator
    {
        IValidationResult CheckFormat(IPassport passport);
        bool CheckValidity(IPassport passport, DateTime onDate);
        bool CheckUniqueness(IPassport passport, List<IPassport> existingPassports);
    }
}
