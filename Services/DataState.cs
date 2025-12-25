using CouseWork3Semester.Registries;
using CouseWork3Semester.Models;
using System.Collections.Generic;

namespace CouseWork3Semester.Services
{
    public class DataState
    {
        public DormitoryRegistry DormitoryRegistry { get; set; }
        public OccupantRegistry OccupantRegistry { get; set; }
        public DocumentRegistry DocumentRegistry { get; set; }
        public SettlementEvictionService SettlementEvictionService { get; set; }
        public InventoryRegistry InventoryRegistry { get; set; }
        public List<DocumentOccupantLink> DocumentLinks { get; set; }
    }
}