using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Interfaces
{
    public interface IRoomOccupant
    {
        Guid Id { get; }
        string FullName { get; }
        DateTime BirthDate { get; }

        int GetAge();
        string GetOccupantType();
    }
}
