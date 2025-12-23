using CouseWork3Semester.Interfaces;
using CouseWork3Semester.Interfaces.IModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Services
{
    public class PermissionManager : IPermissionManager
    {
        // Словарь прав для каждой роли
        private readonly Dictionary<string, List<string>> rolePermissions;

        // Конструктор
        public PermissionManager()
        {
            rolePermissions = new Dictionary<string, List<string>>
            {
                // Администратор - все права
                ["Администратор"] = new List<string>
                {
                    "Просмотр_всех_данных",
                    "Редактирование_общежитий",
                    "Редактирование_комнат",
                    "Редактирование_жильцов",
                    "Управление_оплатами",
                    "Генерация_отчетов",
                    "Управление_сотрудниками",
                    "Управление_правами"
                },

                // Менеджер - большинство прав
                ["Менеджер"] = new List<string>
                {
                    "Просмотр_всех_данных",
                    "Редактирование_комнат",
                    "Редактирование_жильцов",
                    "Управление_оплатами",
                    "Генерация_отчетов"
                },

                // Дежурный - ограниченные права
                ["Дежурный"] = new List<string>
                {
                    "Просмотр_общежитий",
                    "Просмотр_комнат",
                    "Просмотр_жильцов",
                    "Экстренное_редактирование"
                },

                // Бухгалтер - права связанные с финансами
                ["Бухгалтер"] = new List<string>
                {
                    "Просмотр_оплат",
                    "Управление_оплатами",
                    "Генерация_финансовых_отчетов",
                    "Просмотр_жильцов"
                }
            };
        }

        // Реализация методов интерфейса

        public bool CanEmployeeDoAction(IEmployee employee, string action)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));

            if (string.IsNullOrWhiteSpace(action))
                throw new ArgumentException("Действие не может быть пустым", nameof(action));

            // Получаем роль сотрудника
            var role = employee.Role;

            // Проверяем, есть ли такая роль в словаре
            if (!rolePermissions.ContainsKey(role))
                return false;

            // Проверяем, есть ли у этой роли нужное право
            return rolePermissions[role].Contains(action);
        }

        public List<string> GetAllPermissionsForRole(string role)
        {
            if (string.IsNullOrWhiteSpace(role))
                throw new ArgumentException("Роль не может быть пустой", nameof(role));

            // Возвращаем список прав для роли или пустой список
            return rolePermissions.ContainsKey(role)
                ? new List<string>(rolePermissions[role])
                : new List<string>();
        }

        // Дополнительные методы

        public void AddPermissionToRole(string role, string permission)
        {
            if (string.IsNullOrWhiteSpace(role))
                throw new ArgumentException("Роль не может быть пустой", nameof(role));

            if (string.IsNullOrWhiteSpace(permission))
                throw new ArgumentException("Право не может быть пустым", nameof(permission));

            if (!rolePermissions.ContainsKey(role))
            {
                rolePermissions[role] = new List<string>();
            }

            if (!rolePermissions[role].Contains(permission))
            {
                rolePermissions[role].Add(permission);
            }
        }

        public void RemovePermissionFromRole(string role, string permission)
        {
            if (string.IsNullOrWhiteSpace(role) || string.IsNullOrWhiteSpace(permission))
                return;

            if (rolePermissions.ContainsKey(role))
            {
                rolePermissions[role].Remove(permission);
            }
        }

        public List<string> GetAllRoles()
        {
            return rolePermissions.Keys.ToList();
        }

        public void AddNewRole(string role, List<string> permissions)
        {
            if (string.IsNullOrWhiteSpace(role))
                throw new ArgumentException("Роль не может быть пустой", nameof(role));

            if (permissions == null)
                throw new ArgumentNullException(nameof(permissions));

            if (!rolePermissions.ContainsKey(role))
            {
                rolePermissions[role] = new List<string>(permissions);
            }
        }

        public Dictionary<string, List<string>> GetAllRolePermissions()
        {
            // Возвращаем копию словаря
            var copy = new Dictionary<string, List<string>>();
            foreach (var kvp in rolePermissions)
            {
                copy[kvp.Key] = new List<string>(kvp.Value);
            }
            return copy;
        }

        public override string ToString()
        {
            return $"Permission Manager: {rolePermissions.Count} ролей";
        }
    }
}
