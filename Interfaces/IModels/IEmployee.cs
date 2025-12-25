using System;
using System.Collections.Generic;
using System.Text;
using CouseWork3Semester.Enums;

namespace CouseWork3Semester.Interfaces
{
    public interface IEmployee
    {
        string Login { get; }
        string Password { get; }
        string FullName { get; }
        UserRole Role { get; }
    }
}
