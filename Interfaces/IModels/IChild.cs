using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Interfaces.IModels
{
    public interface IChild : IRoomOccupant
    {
        Guid ParentResidentId { get; }
    }
}
