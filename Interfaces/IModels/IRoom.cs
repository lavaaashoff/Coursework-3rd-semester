using System;
using System.Collections.Generic;
using System.Text;
using CouseWork3Semester.Models;

namespace CouseWork3Semester.Interfaces.IModels
{
    public interface IRoom
    {
        int Number { get; }
        double Area { get; }
        int Type { get; }
        int Floor { get; }

        bool AddOccupant(IRoomOccupant resident);
        bool RemoveOccupant(Guid residentId);
        int GetAvailablePlacesCount();
        bool CheckAvailablePlaces();
        List<IRoomOccupant> GetAllOccupants();
    }
}
