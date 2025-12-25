using CouseWork3Semester.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Registries
{
    public class DormitoryRegistry : IDormitoryRegistry
    {
        public List<IDormitory> Dormitories { get; private set; }

        public DormitoryRegistry()
        {
            Dormitories = new List<IDormitory>();
        }

        public DormitoryRegistry(List<IDormitory> initialDormitories)
        {
            if (initialDormitories == null)
                throw new ArgumentNullException(nameof(initialDormitories));

            Dormitories = initialDormitories;

            ValidateUniqueDormitoryNumbers();
        }


        public void AddDormitory(IDormitory dormitory)
        {
            if (dormitory == null)
                throw new ArgumentNullException(nameof(dormitory));

            if (Dormitories.Any(d => d.Number == dormitory.Number))
                throw new InvalidOperationException($"Dormitory with number {dormitory.Number} already exists in registry");

            Dormitories.Add(dormitory);
        }

        public bool RemoveDormitory(int number)
        {
            if (number <= 0)
                throw new ArgumentException("Dormitory number must be positive", nameof(number));

            var dormitoryToRemove = GetDormitoryByNumber(number);

            if (dormitoryToRemove == null)
                return false;

            if (dormitoryToRemove.GetTotalOccupantsCount() > 0)
            {
                throw new InvalidOperationException($"Cannot remove dormitory {number} because it has occupants");
            }

            return Dormitories.Remove(dormitoryToRemove);
        }

        public IDormitory GetDormitoryByNumber(int number)
        {
            if (number <= 0)
                throw new ArgumentException("Dormitory number must be positive", nameof(number));

            return Dormitories.FirstOrDefault(d => d.Number == number);
        }

        public List<IDormitory> GetAllDormitories()
        {
            return new List<IDormitory>(Dormitories.OrderBy(d => d.Number));
        }



        public int GetTotalPlacesCount()
        {
            return Dormitories.Sum(d => d.GetTotalPlacesCount());
        }

        public int GetTotalAvailablePlacesCount()
        {
            return Dormitories.Sum(d => d.GetAvailablePlacesCount());
        }

        public int GetTotalOccupiedPlacesCount()
        {
            return Dormitories.Sum(d => d.GetOccupiedPlacesCount());
        }

        public int GetTotalOccupantsCount()
        {
            return Dormitories.Sum(d => d.GetTotalOccupantsCount());
        }

        public double GetOverallOccupancyPercentage()
        {
            int totalPlaces = GetTotalPlacesCount();

            if (totalPlaces == 0)
                return 0.0;

            int occupiedPlaces = GetTotalOccupiedPlacesCount();
            return Math.Round((double)occupiedPlaces / totalPlaces * 100, 2);
        }

        private void ValidateUniqueDormitoryNumbers()
        {
            var duplicateNumbers = Dormitories
                .GroupBy(d => d.Number)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateNumbers.Any())
            {
                throw new ArgumentException($"Duplicate dormitory numbers found: {string.Join(", ", duplicateNumbers)}");
            }
        }
    }
}
