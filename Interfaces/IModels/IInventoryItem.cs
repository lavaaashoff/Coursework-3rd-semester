using System;

namespace CouseWork3Semester.Interfaces
{
    public interface IInventoryItem
    {
        Guid Id { get; }
        string Name { get; }
        int Quantity { get; }
        int DormitoryNumber { get; }
        int RoomNumber { get; }
    }
}