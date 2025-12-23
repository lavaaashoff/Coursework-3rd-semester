using CouseWork3Semester.Interfaces;
using CouseWork3Semester.Interfaces.IModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Registries
{
    public class DormitoryRegistry : IDormitoryRegistry
    {
        // Публичное свойство для списка общежитий
        public List<IDormitory> Dormitories { get; private set; }

        // Конструктор
        public DormitoryRegistry()
        {
            Dormitories = new List<IDormitory>();
        }

        // Конструктор с начальным списком общежитий
        public DormitoryRegistry(List<IDormitory> initialDormitories)
        {
            if (initialDormitories == null)
                throw new ArgumentNullException(nameof(initialDormitories));

            Dormitories = initialDormitories;

            // Проверяем уникальность номеров
            ValidateUniqueDormitoryNumbers();
        }

        // Реализация методов интерфейса

        public void AddDormitory(IDormitory dormitory)
        {
            if (dormitory == null)
                throw new ArgumentNullException(nameof(dormitory));

            // Проверяем, нет ли уже общежития с таким номером
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

            // Проверяем, что в общежитии нет проживающих
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

        // Дополнительные методы (не из интерфейса, но полезные)

        public List<IDormitory> GetDormitoriesByAddress(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentException("Address cannot be null or empty", nameof(address));

            return Dormitories
                .Where(d => d.Address.Contains(address, StringComparison.OrdinalIgnoreCase))
                .OrderBy(d => d.Number)
                .ToList();
        }

        public IDormitory GetDormitoryWithHighestOccupancy()
        {
            if (Dormitories.Count == 0)
                return null;

            return Dormitories
                .OrderByDescending(d => d.GetOccupancyPercentage())
                .First();
        }

        public IDormitory GetDormitoryWithLowestOccupancy()
        {
            if (Dormitories.Count == 0)
                return null;

            return Dormitories
                .OrderBy(d => d.GetOccupancyPercentage())
                .First();
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

        public List<IDormitory> GetDormitoriesWithAvailablePlaces()
        {
            return Dormitories
                .Where(d => d.GetAvailablePlacesCount() > 0)
                .OrderBy(d => d.Number)
                .ToList();
        }

        public List<IDormitory> GetFullDormitories()
        {
            return Dormitories
                .Where(d => d.GetAvailablePlacesCount() == 0)
                .OrderBy(d => d.Number)
                .ToList();
        }

        // Статистические методы

        public Dictionary<int, int> GetPlacesDistributionByDormitory()
        {
            return Dormitories
                .ToDictionary(
                    d => d.Number,
                    d => d.GetTotalPlacesCount()
                );
        }

        public Dictionary<int, int> GetOccupancyDistributionByDormitory()
        {
            return Dormitories
                .ToDictionary(
                    d => d.Number,
                    d => d.GetOccupiedPlacesCount()
                );
        }

        // Вспомогательные методы

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

        public void Clear()
        {
            // Проверяем, что все общежития пустые
            foreach (var dormitory in Dormitories)
            {
                if (dormitory.GetTotalOccupantsCount() > 0)
                {
                    throw new InvalidOperationException($"Cannot clear registry because dormitory {dormitory.Number} has occupants");
                }
            }

            Dormitories.Clear();
        }

        // Переопределение стандартных методов

        public override string ToString()
        {
            return $"Dormitory Registry: {Dormitories.Count} dormitories, " +
                   $"{GetTotalPlacesCount()} total places, " +
                   $"{GetTotalOccupiedPlacesCount()} occupied places, " +
                   $"{GetOverallOccupancyPercentage():F2}% occupancy";
        }

        public string GetDetailedReport()
        {
            var report = new System.Text.StringBuilder();

            report.AppendLine("DORMITORY REGISTRY DETAILED REPORT");
            report.AppendLine("==================================");
            report.AppendLine($"Total Dormitories: {Dormitories.Count}");
            report.AppendLine($"Total Places: {GetTotalPlacesCount()}");
            report.AppendLine($"Occupied Places: {GetTotalOccupiedPlacesCount()}");
            report.AppendLine($"Available Places: {GetTotalAvailablePlacesCount()}");
            report.AppendLine($"Total Occupants: {GetTotalOccupantsCount()}");
            report.AppendLine($"Overall Occupancy: {GetOverallOccupancyPercentage():F2}%");
            report.AppendLine();

            foreach (var dormitory in GetAllDormitories())
            {
                report.AppendLine($"Dormitory #{dormitory.Number}");
                report.AppendLine($"Address: {dormitory.Address}");
                report.AppendLine($"Rooms: {dormitory.GetAllRooms().Count}");
                report.AppendLine($"Places: {dormitory.GetTotalPlacesCount()} " +
                                 $"(Occupied: {dormitory.GetOccupiedPlacesCount()}, " +
                                 $"Available: {dormitory.GetAvailablePlacesCount()})");
                report.AppendLine($"Occupancy: {dormitory.GetOccupancyPercentage():F2}%");
                report.AppendLine();
            }

            return report.ToString();
        }
    }
}
