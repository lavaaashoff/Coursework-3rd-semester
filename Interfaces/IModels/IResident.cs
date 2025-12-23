using CouseWork3Semester.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using CouseWork3Semester.Models;

namespace CouseWork3Semester.Interfaces.IModels
{
    public interface IResident : IRoomOccupant
    {
        int RegistrationNumber { get; }
        Gender Gender { get; }
        Passport Passport { get; }
        bool WorkStatus { get; set; }
        string? Workplace { get; set; }
        bool StudyStatus { get; set; }
        string? StudyPlace { get; set; }
        DateTime CheckInDate { get; }



        string GetFullInfo();
    }
}
