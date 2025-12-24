using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Interfaces
{
    public interface IInventoryLedger
    {
        // Свойства
        IRoom Room { get; }
        Dictionary<IInventoryType, int> Inventory { get; }

        // Методы
        void AddInventory(IInventoryType type, int quantity);
        void RemoveInventory(IInventoryType type);
        void ChangeInventoryQuantity(IInventoryType type, int newQuantity);
        Dictionary<IInventoryType, int> GetInventoryReport();
    }
}
