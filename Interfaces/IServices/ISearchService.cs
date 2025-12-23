using CouseWork3Semester.Interfaces.IModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Interfaces
{
    public interface ISearchService
    {
        IDormitoryRegistry DormitoryRegistry { get; }
        IOccupantRegistry OccupantRegistry { get; }

        List<IRoomOccupant> FindOccupant(string fullName);
        List<IRoom> FindAvailableRooms(IDormitory dormitory);
        List<IDormitory> FindDormitoryByAddress(string address);
    }
}
