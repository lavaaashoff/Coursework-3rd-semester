﻿using CouseWork3Semester.Interfaces;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CouseWork3Semester.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Dormitory : IDormitory
    {
        [JsonProperty]
        public int Number { get; private set; }

        [JsonProperty]
        public string Address { get; private set; }

        [JsonProperty]
        public string PhotoPath { get; private set; }

        [JsonProperty]
        private List<IRoom> rooms;

        [JsonConstructor]
        public Dormitory(int number, string address, string photoPath = "")
        {
            ValidateConstructorParameters(number, address);

            Number = number;
            Address = address;
            PhotoPath = photoPath ?? string.Empty;
            rooms = rooms ?? new List<IRoom>();
        }

        private void ValidateConstructorParameters(int number, string address)
        {
            if (number <= 0)
                throw new ArgumentException("Dormitory number must be positive", nameof(number));

            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentException("Address cannot be null or empty", nameof(address));

            if (address.Length < 5)
                throw new ArgumentException("Address is too short", nameof(address));
        }

        public void AddRoom(IRoom room)
        {
            if (room == null)
                throw new ArgumentNullException(nameof(room));

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
                PhotoPath = string.Empty;
                return true;
            }

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
                totalPlaces += room.Type;
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

        public int GetTotalOccupantsCount()
        {
            return rooms.Sum(room => room.GetAllOccupants().Count);
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
            return rooms.FindAll(r => r.CheckAvailablePlaces());
        }

        public List<IRoom> GetAllRooms()
        {
            return new List<IRoom>(rooms);
        }

        private bool IsValidPhotoUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var _);
        }


    }
}