using CouseWork3Semester.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Models
{
    public class Room : IRoom
    {
        public int Number { get; private set; }
        public double Area { get; private set; }
        public int Type { get; private set; }
        public int Floor { get; private set; }

        private List<IRoomOccupant> occupants;

        public Room(int number, double area, int type, int floor)
        {
            Number = number;
            Area = area;
            Type = type;
            Floor = floor;
            occupants = new List<IRoomOccupant>();
        }


        public bool AddOccupant(IRoomOccupant occupant)
        {
            if (occupant == null)
                throw new ArgumentNullException(nameof(occupant));

            if (!CheckAvailablePlaces())
                return false;

            if (occupants.Exists(r => r.Id == occupant.Id))
                return false;

            occupants.Add(occupant);
            return true;
        }

        public bool RemoveOccupant(Guid occupantId)
        {
            IRoomOccupant occupantToRemove = occupants.Find(o => o.Id == occupantId);
            if (occupantToRemove != null)
            {
                return occupants.Remove(occupantToRemove);
            }
            return false;
        }

        public int GetAvailablePlacesCount()
        {
            int maxOccupants = Type;
            return maxOccupants - occupants.Count;
        }

        public bool CheckAvailablePlaces()
        {
            return GetAvailablePlacesCount() > 0;
        }

        public List<IRoomOccupant> GetAllOccupants()
        {
            return new List<IRoomOccupant>(occupants);
        }
    }
}
