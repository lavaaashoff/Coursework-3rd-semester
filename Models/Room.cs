using CouseWork3Semester.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Models
{
    public class Room : IRoom
    {
        // Свойства
        public int Number { get; private set; }
        public double Area { get; private set; }
        public int Type { get; private set; }
        public int Floor { get; private set; }

        // Приватное поле для списка жильцов
        private List<Resident> residents;

        // Конструктор
        public Room(int number, double area, int type, int floor)
        {
            Number = number;
            Area = area;
            Type = type;
            Floor = floor;
            residents = new List<Resident>();
        }

        // Методы интерфейса

        public bool AddResident(Resident resident)
        {
            if (resident == null)
                throw new ArgumentNullException(nameof(resident));

            // Проверяем, есть ли свободные места
            if (!CheckAvailablePlaces())
                return false;

            // Проверяем, не живет ли уже такой жилец в комнате
            if (residents.Exists(r => r.Id == resident.Id))
                return false;

            residents.Add(resident);
            return true;
        }

        public bool RemoveResident(Guid residentId)
        {
            Resident residentToRemove = residents.Find(r => r.Id == residentId);
            if (residentToRemove != null)
            {
                return residents.Remove(residentToRemove);
            }
            return false;
        }

        public int GetAvailablePlacesCount()
        {
            int maxResidents = Type;
            return maxResidents - residents.Count;
        }

        public bool CheckAvailablePlaces()
        {
            return GetAvailablePlacesCount() > 0;
        }

        public List<Resident> GetResidentsList()
        {
            // Возвращаем копию списка, чтобы защитить исходные данные
            return new List<Resident>(residents);
        }
    }
}
