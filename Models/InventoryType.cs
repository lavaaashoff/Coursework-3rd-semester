using CouseWork3Semester.Interfaces.IModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Models
{
    public class InventoryType : IInventoryType
    {
        // Свойства
        public Guid Id { get; set; }

        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Name cannot be null or empty", nameof(Name));

                _name = value.Trim();
            }
        }

        private string _category;
        public string Category
        {
            get => _category;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Category cannot be null or empty", nameof(Category));

                _category = value.Trim();
            }
        }

        private string _description;
        public string Description
        {
            get => _description;
            set => _description = value?.Trim() ?? string.Empty;
        }

        // Конструкторы
        public InventoryType(string name, string category, string description = null)
        {
            Id = Guid.NewGuid();
            Name = name;
            Category = category;
            Description = description ?? string.Empty;
        }

        // Конструктор для загрузки из БД (с существующим Id)
        //public InventoryType(Guid id, string name, string category, string description)
        //{
        //    Id = id;
        //    Name = name;
        //    Category = category;
        //    Description = description ?? string.Empty;
        //}

        public string GetFullInfo()
        {
            return $"Inventory Type ID: {Id}\n" +
                   $"Name: {Name}\n" +
                   $"Category: {Category}\n" +
                   $"Description: {Description}";
        }

        // Дополнительные методы
        public string GetShortInfo()
        {
            return $"{Name} ({Category})";
        }

        public override bool Equals(object obj)
        {
            if (obj is InventoryType other)
            {
                return Id.Equals(other.Id) ||
                       (Name == other.Name && Category == other.Category);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Category);
        }
    }
}
