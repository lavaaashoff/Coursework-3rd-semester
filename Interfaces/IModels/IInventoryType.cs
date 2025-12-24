using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Interfaces
{
    public interface IInventoryType
    {
        // Свойства
        Guid Id { get; set; }
        string Name { get; set; }
        string Category { get; set; }
        string Description { get; set; }

        // Методы
        string GetFullInfo();
    }
}
