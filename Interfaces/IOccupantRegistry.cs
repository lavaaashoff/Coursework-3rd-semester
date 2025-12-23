using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Interfaces
{
    public interface IResidentRegistry
    {
        // Свойство из UML
        Dictionary<Guid, IResident> Residents { get; }

        // Методы из UML
        void AddResident(IResident resident);
        IResident FindResidentById(Guid id);

        // Дополнительные полезные методы
        List<IResident> GetAllResidents();
        bool RemoveResident(Guid id);
        bool ContainsResident(Guid id);
        int GetResidentsCount();

        // Методы поиска
        List<IResident> FindResidentsByName(string name);
        List<IResident> GetResidentsByAgeRange(int minAge, int maxAge);
    }
}
