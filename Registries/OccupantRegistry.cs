using CouseWork3Semester.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Registries
{
    public class OccupantRegistry : IOccupantRegistry
    {
        public Dictionary<Guid, IRoomOccupant> AllOccupants { get; private set; }

        public OccupantRegistry()
        {
            AllOccupants = new Dictionary<Guid, IRoomOccupant>();
        }


        public void AddOccupant(IRoomOccupant occupant)
        {
            if (occupant == null)
                throw new ArgumentNullException(nameof(occupant));

            if (AllOccupants.ContainsKey(occupant.Id))
                throw new InvalidOperationException($"Occupant with ID {occupant.Id} already exists");

            AllOccupants.Add(occupant.Id, occupant);
        }

        public IRoomOccupant FindOccupantById(Guid id)
        {
            AllOccupants.TryGetValue(id, out IRoomOccupant occupant);
            return occupant;
        }


        public void AddResident(IResident resident)
        {
            AddOccupant(resident);
        }

        public void AddChild(IChild child)
        {
            var parent = GetAllResidents().FirstOrDefault(r => r.Id == child.ParentResidentId);
            if (parent == null)
                throw new InvalidOperationException($"Parent resident with ID {child.ParentResidentId} not found");

            AddOccupant(child);
        }


        public List<IRoomOccupant> GetAllOccupants()
        {
            return AllOccupants.Values.ToList();
        }

        public List<IResident> GetAllResidents()
        {
            return AllOccupants.Values
                .Where(o => o is IResident)
                .Cast<IResident>()
                .ToList();
        }

        public List<IChild> GetAllChildren()
        {
            return AllOccupants.Values
                .Where(o => o is IChild)
                .Cast<IChild>()
                .ToList();
        }


        public bool RemoveOccupant(Guid id)
        {
            return AllOccupants.Remove(id);
        }

        public bool ContainsOccupant(Guid id)
        {
            return AllOccupants.ContainsKey(id);
        }

        public int GetOccupantsCount()
        {
            return AllOccupants.Count;
        }

        public int GetResidentsCount()
        {
            return GetAllResidents().Count;
        }

        public int GetChildrenCount()
        {
            return GetAllChildren().Count;
        }

        public List<IChild> GetChildrenOfResident(Guid parentId)
        {
            return GetAllChildren()
                .Where(c => c.ParentResidentId == parentId)
                .ToList();
        }
    }
}
