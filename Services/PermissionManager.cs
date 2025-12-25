using System;
using System.Collections.Generic;
using System.Linq;
using CouseWork3Semester.Interfaces;
using CouseWork3Semester.Enums;

namespace CouseWork3Semester.Services
{
    public class PermissionManager : IPermissionManager
    {
        private readonly Dictionary<UserRole, List<string>> rolePermissions;

        public PermissionManager()
        {
            rolePermissions = new Dictionary<UserRole, List<string>>
            {
                [UserRole.Administrator] = new List<string>
                {
                    "ManageDormitories", "EditDormitories",
                    "ManageRooms", "EditRooms",
                    "ManageOccupants", "EditOccupants",
                    "ManageDocuments", "EditDocuments",
                    "ManageEvictions", "EditEvictions",
                    "ManageInventory", "EditInventory",
                    "GenerateReports", "ViewAllData"
                },
                [UserRole.Commandant] = new List<string>
                {
                    "ManageOccupants", "EditOccupants",
                    "ManageRooms", "EditRoomStatus",
                    "ManageDocuments", "EditDocuments",
                    "ManageEvictions", "EditEvictions",
                    "ManageInventory", "EditInventory",
                    "GenerateReportsForMyData", "ViewOwnData"
                },
                [UserRole.AdminStaff] = new List<string>
                {
                    "ViewAllData",
                    "GenerateReports",
                    "FilterData"
                }
            };
        }

        public bool CanRolePerformAction(UserRole role, string action)
        {
            if (rolePermissions.ContainsKey(role))
            {
                return rolePermissions[role].Contains(action);
            }
            return false; 
        }

        public List<string> GetAllPermissionsForRole(UserRole role)
        {
            if (rolePermissions.ContainsKey(role))
            {
                return rolePermissions[role];
            }
            return new List<string>();
        }
    }
}