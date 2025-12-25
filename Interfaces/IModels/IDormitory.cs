using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Interfaces
{
    public interface IDormitory
    {
        int Number { get; }
        string Address { get; }
        string PhotoPath { get; }

        void AddRoom(IRoom room);
        bool RemoveRoom(int roomNumber);
        bool UpdatePhoto(string photoPath);

        int GetTotalPlacesCount();
        int GetAvailablePlacesCount();
        double GetOccupancyPercentage();
        List<IRoom> GetAvailableRoomsList();

        int GetTotalOccupantsCount();

        List<IRoom> GetAllRooms();
        int GetOccupiedPlacesCount();
    }
}
