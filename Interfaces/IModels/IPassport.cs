using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Interfaces.IModels
{
    public interface IPassport
    {
        string Series { get; }
        string Number { get; }
        DateTime IssueDate { get; }
        string IssuedBy { get; }

        string GetFullPassportData();
    }
}
