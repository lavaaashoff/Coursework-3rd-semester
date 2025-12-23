using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Interfaces
{
    public interface IDormitory
    {
        // Свойства
        int Number { get; }
        string Address { get; }
        string PhotoPath { get; }

        // Основные методы
        void AddRoom(IRoom room);
        bool RemoveRoom(int roomNumber);
        bool UpdatePhoto(string photoPath);

        // Методы получения информации
        int GetTotalPlacesCount();
        int GetAvailablePlacesCount();
        double GetOccupancyPercentage();
        List<IRoom> GetAvailableRoomsList();

        int GetTotalOccupantsCount();

        // Дополнительные методы (опционально)
        List<IRoom> GetAllRooms();
        int GetOccupiedPlacesCount();
    }
}
