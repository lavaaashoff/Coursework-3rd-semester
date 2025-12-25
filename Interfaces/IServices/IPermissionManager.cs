using CouseWork3Semester.Interfaces;
using CouseWork3Semester.Enums;
using System;

using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Interfaces
{
    public interface IPermissionManager
    {
        public bool CanRolePerformAction(UserRole role, string action);
        public List<string> GetAllPermissionsForRole(UserRole role);
    }
}
