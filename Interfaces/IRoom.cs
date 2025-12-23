using System;
using System.Collections.Generic;
using System.Text;
using CouseWork3Semester.Models;

namespace CouseWork3Semester.Interfaces
{
    public interface IRoom
    {
        int Number { get; }
        double Area { get; }
        int Type { get; }
        int Floor { get; }

        bool AddResident(Resident resident);
        bool RemoveResident(Guid residentId);
        int GetAvailablePlacesCount();
        bool CheckAvailablePlaces();
        List<Resident> GetResidentsList();
    }
}
