using CouseWork3Semester.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Interfaces
{
    public interface IPermissionManager
    {
        // Методы из UML
        bool CanEmployeeDoAction(IEmployee employee, string action);
        List<string> GetAllPermissionsForRole(string role);
    }
}
