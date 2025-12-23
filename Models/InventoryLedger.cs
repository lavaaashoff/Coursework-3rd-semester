using CouseWork3Semester.Interfaces;
using CouseWork3Semester.Interfaces.IModels;
using System;
using System.Collections.Generic;

namespace CouseWork3Semester.Models
{
    public class InventoryLedger : IInventoryLedger
    {
        // Реализация свойств интерфейса
        public IRoom Room { get; private set; }
        public Dictionary<IInventoryType, int> Inventory { get; private set; }

        // Конструктор
        public InventoryLedger(IRoom room)
        {
            Room = room ?? throw new ArgumentNullException(nameof(room));
            Inventory = new Dictionary<IInventoryType, int>();
        }

        // Реализация методов интерфейса

        public void AddInventory(IInventoryType type, int quantity)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive", nameof(quantity));

            if (Inventory.ContainsKey(type))
            {
                Inventory[type] += quantity;
            }
            else
            {
                Inventory[type] = quantity;
            }
        }

        public void RemoveInventory(IInventoryType type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            Inventory.Remove(type);
        }

        public void ChangeInventoryQuantity(IInventoryType type, int newQuantity)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (newQuantity < 0)
                throw new ArgumentException("Quantity cannot be negative", nameof(newQuantity));

            if (newQuantity == 0)
            {
                RemoveInventory(type);
            }
            else
            {
                Inventory[type] = newQuantity;
            }
        }

        public Dictionary<IInventoryType, int> GetInventoryReport()
        {
            return new Dictionary<IInventoryType, int>(Inventory);
        }

        // Дополнительные методы (не из интерфейса, но полезные)

        public int GetTotalCount()
        {
            int total = 0;
            foreach (var quantity in Inventory.Values)
            {
                total += quantity;
            }
            return total;
        }
    }
}