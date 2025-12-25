using CouseWork3Semester.Interfaces;
using CouseWork3Semester.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Interfaces
{
    public interface IOccupantRegistry
    {
        Dictionary<Guid, IRoomOccupant> AllOccupants { get; }

        void AddOccupant(IRoomOccupant occupant);
        IRoomOccupant FindOccupantById(Guid id);

        void AddResident(IResident resident);
        void AddChild(IChild child);

        List<IRoomOccupant> GetAllOccupants();
        List<IResident> GetAllResidents();
        List<IChild> GetAllChildren();
        List<IChild> GetChildrenOfResident(Guid parentId);

        bool RemoveOccupant(Guid id);
        bool ContainsOccupant(Guid id);
        int GetOccupantsCount();
        int GetResidentsCount();
        int GetChildrenCount();
    }
}
