using System;
using System.Collections.Generic;

namespace CouseWork3Semester.Interfaces
{
    public interface IInventoryRegistry
    {
        // Основные CRUD-операции
        void AddItem(IInventoryItem item);
        bool UpdateItem(Guid id, string name, int quantity);
        bool RemoveItem(Guid id);

        // Поиск/получение
        IInventoryItem GetItemById(Guid id);
        List<IInventoryItem> GetItemsForRoom(int dormitoryNumber, int roomNumber);
        List<IInventoryItem> GetAllItems();

        // Дополнительно
        bool ContainsItem(Guid id);
    }
}