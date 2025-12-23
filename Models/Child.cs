using CouseWork3Semester.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Models
{
    public class Child : IRoomOccupant
    {
        public Guid Id { get; private set; }
        public string FullName { get; private set; }
        public DateTime BirthDate { get; private set; }

        // Дополнительные свойства для ребенка
        public string BirthCertificateNumber { get; private set; }
        public Guid ParentResidentId { get; private set; }

        public Child(string fullName, DateTime birthDate, string birthCertificateNumber,
                     Guid parentResidentId)
        {
            Id = Guid.NewGuid();
            FullName = fullName;
            BirthDate = birthDate;
            BirthCertificateNumber = birthCertificateNumber;
            ParentResidentId = parentResidentId;
        }

        public int GetAge()
        {
            var today = DateTime.Today;
            var age = today.Year - BirthDate.Year;

            if (BirthDate.Date > today.AddYears(-age))
            {
                age--;
            }

            return age;
        }

        public string GetOccupantType()
        {
            return "Child";
        }
    }
}
