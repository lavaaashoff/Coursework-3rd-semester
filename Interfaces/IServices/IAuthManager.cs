using CouseWork3Semester.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Interfaces
{
    public interface IAuthManager
    {
        // Методы из UML
        IEmployee Login(string login, string password);
        void ChangePassword(IEmployee employee, string oldPassword, string newPassword);
        void Logout();
    }
}
