using CouseWork3Semester.Interfaces;
using CouseWork3Semester.Interfaces.IModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Registries
{
    public class OccupantRegistry : IOccupantRegistry
    {
        // Храним ВСЕХ обитателей через базовый интерфейс
        public Dictionary<Guid, IRoomOccupant> AllOccupants { get; private set; }

        // Конструктор
        public OccupantRegistry()
        {
            AllOccupants = new Dictionary<Guid, IRoomOccupant>();
        }

        // Основные методы

        public void AddOccupant(IRoomOccupant occupant)
        {
            if (occupant == null)
                throw new ArgumentNullException(nameof(occupant));

            if (AllOccupants.ContainsKey(occupant.Id))
                throw new InvalidOperationException($"Occupant with ID {occupant.Id} already exists");

            AllOccupants.Add(occupant.Id, occupant);
        }

        public IRoomOccupant FindOccupantById(Guid id)
        {
            AllOccupants.TryGetValue(id, out IRoomOccupant occupant);
            return occupant;
        }

        // Специализированные методы добавления

        public void AddResident(IResident resident)
        {
            AddOccupant(resident);
        }

        public void AddChild(IChild child)
        {
            // Проверяем, что родитель существует
            var parent = GetAllResidents().FirstOrDefault(r => r.Id == child.ParentResidentId);
            if (parent == null)
                throw new InvalidOperationException($"Parent resident with ID {child.ParentResidentId} not found");

            AddOccupant(child);
        }

        // Получение списков по типам

        public List<IRoomOccupant> GetAllOccupants()
        {
            return AllOccupants.Values.ToList();
        }

        public List<IResident> GetAllResidents()
        {
            return AllOccupants.Values
                .Where(o => o is IResident)
                .Cast<IResident>()
                .ToList();
        }

        public List<IChild> GetAllChildren()
        {
            return AllOccupants.Values
                .Where(o => o is IChild)
                .Cast<IChild>()
                .ToList();
        }


        // Дополнительные методы

        public bool RemoveOccupant(Guid id)
        {
            return AllOccupants.Remove(id);
        }

        public bool ContainsOccupant(Guid id)
        {
            return AllOccupants.ContainsKey(id);
        }

        public int GetOccupantsCount()
        {
            return AllOccupants.Count;
        }

        public int GetResidentsCount()
        {
            return GetAllResidents().Count;
        }

        public int GetChildrenCount()
        {
            return GetAllChildren().Count;
        }

        // Поисковые методы

        public List<IRoomOccupant> FindOccupantsByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return new List<IRoomOccupant>();

            string searchName = name.ToLower();

            return AllOccupants.Values
                .Where(o => o.FullName.ToLower().Contains(searchName))
                .ToList();
        }

        public List<IRoomOccupant> GetOccupantsByAgeRange(int minAge, int maxAge)
        {
            return AllOccupants.Values
                .Where(o => o.GetAge() >= minAge && o.GetAge() <= maxAge)
                .ToList();
        }

        // Получение детей конкретного жильца
        public List<IChild> GetChildrenOfResident(Guid parentId)
        {
            return GetAllChildren()
                .Where(c => c.ParentResidentId == parentId)
                .ToList();
        }

        // Получение родителя ребенка
        public IResident GetParentOfChild(IChild child)
        {
            return GetAllResidents()
                .FirstOrDefault(r => r.Id == child.ParentResidentId);
        }


        public override string ToString()
        {
            return $"Occupant Registry: {GetOccupantsCount()} total ({GetResidentsCount()} adults, {GetChildrenCount()} children)";
        }
    }
}
