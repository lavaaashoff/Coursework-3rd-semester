using System;
using System.Collections.Generic;

namespace CouseWork3Semester.Interfaces
{
    public interface IInventoryRegistry
    {
        void AddItem(IInventoryItem item);
        bool UpdateItem(Guid id, string name, int quantity);
        bool RemoveItem(Guid id);

        IInventoryItem GetItemById(Guid id);
        List<IInventoryItem> GetItemsForRoom(int dormitoryNumber, int roomNumber);
        List<IInventoryItem> GetAllItems();

        bool ContainsItem(Guid id);
    }
}