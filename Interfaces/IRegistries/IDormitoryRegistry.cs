using CouseWork3Semester.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Interfaces
{
    public interface IDormitoryRegistry
    {
        // Свойства
        List<IDormitory> Dormitories { get; }

        // Основные методы
        void AddDormitory(IDormitory dormitory);
        bool RemoveDormitory(int number);
        IDormitory GetDormitoryByNumber(int number);
        List<IDormitory> GetAllDormitories();
        int GetTotalPlacesCount();
        int GetTotalOccupiedPlacesCount();
        double GetOverallOccupancyPercentage();
        int GetTotalAvailablePlacesCount();
    }
}
