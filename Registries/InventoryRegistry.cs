using System;
using System.Collections.Generic;
using System.Linq;
using CouseWork3Semester.Interfaces;

namespace CouseWork3Semester.Registries
{
    public class InventoryRegistry : IInventoryRegistry
    {
        private readonly Dictionary<Guid, IInventoryItem> _items = new();

        public void AddItem(IInventoryItem item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (_items.ContainsKey(item.Id))
                throw new InvalidOperationException($"Item with id {item.Id} already exists");
            _items.Add(item.Id, item);
        }

        public bool UpdateItem(Guid id, string name, int quantity)
        {
            if (!_items.TryGetValue(id, out var item)) return false;

            if (item is CouseWork3Semester.Models.InventoryItem concrete)
            {
                return concrete.Update(name, quantity);
            }
            var updated = new CouseWork3Semester.Models.InventoryItem(name, quantity, item.DormitoryNumber, item.RoomNumber);
            typeof(CouseWork3Semester.Models.InventoryItem)
                .GetProperty(nameof(CouseWork3Semester.Models.InventoryItem.Id))!
                .SetValue(updated, id);
            _items[id] = updated;
            return true;
        }

        public bool RemoveItem(Guid id) => _items.Remove(id);

        public IInventoryItem GetItemById(Guid id)
        {
            _items.TryGetValue(id, out var item);
            return item;
        }

        public List<IInventoryItem> GetItemsForRoom(int dormitoryNumber, int roomNumber)
        {
            return _items.Values
                .Where(i => i.DormitoryNumber == dormitoryNumber && i.RoomNumber == roomNumber)
                .OrderBy(i => i.Name)
                .ToList();
        }

        public List<IInventoryItem> GetAllItems()
        {
            return _items.Values.OrderBy(i => i.DormitoryNumber).ThenBy(i => i.RoomNumber).ThenBy(i => i.Name).ToList();
        }

        public bool ContainsItem(Guid id) => _items.ContainsKey(id);
    }
}