using CouseWork3Semester.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Models
{
    public class Dormitory : IDormitory
    {
        // Реализация свойств интерфейса
        public int Number { get; private set; }
        public string Address { get; private set; }
        public string PhotoPath { get; private set; }

        // Приватное поле для списка комнат
        private List<IRoom> rooms;

        // Конструкторы

        public Dormitory(int number, string address)
        {
            ValidateConstructorParameters(number, address);

            Number = number;
            Address = address;
            PhotoPath = string.Empty;
            rooms = new List<IRoom>();
        }

        public Dormitory(int number, string address, string photoPath) : this(number, address)
        {
            if (!string.IsNullOrWhiteSpace(photoPath))
            {
                PhotoPath = photoPath;
            }
        }

        // Валидация параметров конструктора
        private void ValidateConstructorParameters(int number, string address)
        {
            if (number <= 0)
                throw new ArgumentException("Dormitory number must be positive", nameof(number));

            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentException("Address cannot be null or empty", nameof(address));

            if (address.Length < 5)
                throw new ArgumentException("Address is too short", nameof(address));
        }

        // Реализация методов интерфейса

        public void AddRoom(IRoom room)
        {
            if (room == null)
                throw new ArgumentNullException(nameof(room));

            // Проверяем, нет ли уже комнаты с таким номером
            if (rooms.Exists(r => r.Number == room.Number))
                throw new InvalidOperationException($"Room with number {room.Number} already exists in dormitory {Number}");

            rooms.Add(room);
        }

        public bool RemoveRoom(int roomNumber)
        {
            if (roomNumber <= 0)
                throw new ArgumentException("Room number must be positive", nameof(roomNumber));

            IRoom roomToRemove = rooms.Find(r => r.Number == roomNumber);

            if (roomToRemove == null)
                return false;

            // Проверяем, что комната пустая
            if (roomToRemove.GetAllOccupants().Count > 0)
            {
                throw new InvalidOperationException($"Cannot remove room {roomNumber} because it has occupants");
            }

            return rooms.Remove(roomToRemove);
        }

        public bool UpdatePhoto(string photoUrl)
        {
            if (string.IsNullOrWhiteSpace(photoUrl))
            {
                // Разрешаем очистку фотографии
                PhotoPath = string.Empty;
                return true;
            }

            // Проверяем, является ли строка валидным URL изображения
            if (IsValidPhotoUrl(photoUrl))
            {
                PhotoPath = photoUrl;
                return true;
            }

            return false;
        }

        public int GetTotalPlacesCount()
        {
            int totalPlaces = 0;

            foreach (var room in rooms)
            {
                totalPlaces += room.Type; // Type определяет количество мест
            }

            return totalPlaces;
        }

        public int GetAvailablePlacesCount()
        {
            int availablePlaces = 0;

            foreach (var room in rooms)
            {
                availablePlaces += room.GetAvailablePlacesCount();
            }

            return availablePlaces;
        }

        public int GetOccupiedPlacesCount()
        {
            int occupiedPlaces = 0;

            foreach (var room in rooms)
            {
                occupiedPlaces += (room.Type - room.GetAvailablePlacesCount());
            }

            return occupiedPlaces;
        }

        public double GetOccupancyPercentage()
        {
            int totalPlaces = GetTotalPlacesCount();

            if (totalPlaces == 0)
                return 0.0;

            int occupiedPlaces = GetOccupiedPlacesCount();

            return Math.Round((double)occupiedPlaces / totalPlaces * 100, 2);
        }

        public List<IRoom> GetAvailableRoomsList()
        {
            return rooms
                .Where(r => r.CheckAvailablePlaces())
                .OrderBy(r => r.Number)
                .ToList();
        }

        // Дополнительные методы из интерфейса

        public List<IRoom> GetAllRooms()
        {
            return new List<IRoom>(rooms.OrderBy(r => r.Number));
        }

        // Дополнительные методы (не из интерфейса, но полезные)

        public IRoom FindRoom(int roomNumber)
        {
            return rooms.FirstOrDefault(r => r.Number == roomNumber);
        }

        public List<IRoom> GetRoomsByFloor(int floor)
        {
            return rooms
                .Where(r => r.Floor == floor)
                .OrderBy(r => r.Number)
                .ToList();
        }



        public List<IRoomOccupant> GetAllOccupants()
        {
            var allOccupants = new List<IRoomOccupant>();

            foreach (var room in rooms)
            {
                allOccupants.AddRange(room.GetAllOccupants());
            }

            return allOccupants;
        }

        public int GetTotalOccupantsCount()
        {
            return rooms.Sum(room => room.GetAllOccupants().Count);
        }

        // Вспомогательные методы

        private bool IsValidPhotoUrl(string url)
        {
            // Если строка пустая - это валидно (чтобы можно было очистить фото)
            if (string.IsNullOrWhiteSpace(url))
                return true;

            // Пытаемся создать URL из строки
            if (Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult))
            {
                // Проверяем, что это HTTP или HTTPS ссылка
                return uriResult.Scheme == Uri.UriSchemeHttp ||
                       uriResult.Scheme == Uri.UriSchemeHttps;
            }

            return false;
        }

        // Переопределение стандартных методов

        public string GetDetailedInfo()
        {
            return $"Dormitory Information:\n" +
                   $"----------------------\n" +
                   $"Number: {Number}\n" +
                   $"Address: {Address}\n" +
                   $"Total Rooms: {rooms.Count}\n" +
                   $"Total Places: {GetTotalPlacesCount()}\n" +
                   $"Occupied Places: {GetOccupiedPlacesCount()}\n" +
                   $"Available Places: {GetAvailablePlacesCount()}\n" +
                   $"Occupancy: {GetOccupancyPercentage()}%\n" +
                   $"Photo: {(string.IsNullOrEmpty(PhotoPath) ? "Not set" : PhotoPath)}";
        }
    }
}
