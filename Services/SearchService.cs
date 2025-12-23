using CouseWork3Semester.Interfaces;
using CouseWork3Semester.Interfaces.IModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Services
{
    public class SearchService : ISearchService
    {
        public IDormitoryRegistry DormitoryRegistry { get; }
        public IOccupantRegistry OccupantRegistry { get; }

        public SearchService(
            IDormitoryRegistry dormitoryRegistry,
            IOccupantRegistry occupantRegistry)
        {
            DormitoryRegistry = dormitoryRegistry ?? throw new ArgumentNullException(nameof(dormitoryRegistry));
            OccupantRegistry = occupantRegistry ?? throw new ArgumentNullException(nameof(occupantRegistry));
        }

        public List<IRoomOccupant> FindOccupant(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return new List<IRoomOccupant>();

            var searchName = fullName.ToLower();
            var allOccupants = OccupantRegistry.GetAllOccupants();

            return allOccupants
                .Where(o => o.FullName.ToLower().Contains(searchName))
                .OrderBy(o => o.FullName)
                .ToList();
        }

        public List<IRoom> FindAvailableRooms(IDormitory dormitory)
        {
            if (dormitory == null)
                throw new ArgumentNullException(nameof(dormitory));

            return dormitory.GetAvailableRoomsList();
        }

        public List<IDormitory> FindDormitoryByAddress(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                return new List<IDormitory>();

            return DormitoryRegistry.GetAllDormitories()
                .Where(d => d.Address.ToLower().Contains(address.ToLower()))
                .OrderBy(d => d.Number)
                .ToList();
        }
    }
}
