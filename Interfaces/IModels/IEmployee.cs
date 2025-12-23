using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Interfaces.IModels
{
    public interface IEmployee
    {
        // Свойства из UML
        string Login { get; }
        string Password { get; }
        string FullName { get; }
        string Role { get; }

        // Методы из UML
        string GetData();
        string GetRole();
    }
}
