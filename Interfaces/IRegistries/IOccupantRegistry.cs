using CouseWork3Semester.Interfaces.IModels;
using CouseWork3Semester.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Interfaces
{
    public interface IOccupantRegistry
    {
        // Храним ВСЕХ обитателей, не только взрослых
        Dictionary<Guid, IRoomOccupant> AllOccupants { get; }

        // Основные методы
        void AddOccupant(IRoomOccupant occupant);
        IRoomOccupant FindOccupantById(Guid id);

        // Методы для работы с конкретными типами
        void AddResident(IResident resident);
        void AddChild(IChild child);

        // Поиск по типу
        List<IRoomOccupant> GetAllOccupants();
        List<IResident> GetAllResidents();
        List<IChild> GetAllChildren();
        List<IChild> GetChildrenOfResident(Guid parentId);

        // Дополнительные методы
        bool RemoveOccupant(Guid id);
        bool ContainsOccupant(Guid id);
        int GetOccupantsCount();
        int GetResidentsCount();
        int GetChildrenCount();
    }
}
