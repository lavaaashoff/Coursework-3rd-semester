using System;
using CouseWork3Semester.Interfaces;

namespace CouseWork3Semester.Models
{
    public class InventoryItem : IInventoryItem
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public int Quantity { get; private set; }
        public int DormitoryNumber { get; private set; }
        public int RoomNumber { get; private set; }

        public InventoryItem(string name, int quantity, int dormitoryNumber, int roomNumber)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Item name cannot be empty", nameof(name));
            if (quantity < 0)
                throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be >= 0");
            if (dormitoryNumber <= 0)
                throw new ArgumentOutOfRangeException(nameof(dormitoryNumber));
            if (roomNumber <= 0)
                throw new ArgumentOutOfRangeException(nameof(roomNumber));

            Id = Guid.NewGuid();
            Name = name.Trim();
            Quantity = quantity;
            DormitoryNumber = dormitoryNumber;
            RoomNumber = roomNumber;
        }

        public bool Update(string name, int quantity)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;
            if (quantity < 0)
                return false;

            Name = name.Trim();
            Quantity = quantity;
            return true;
        }
    }
}