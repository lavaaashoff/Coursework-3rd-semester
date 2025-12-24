using CouseWork3Semester.Enums;
using CouseWork3Semester.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Models
{
    // Класс жильца, реализующий интерфейс
    public class Resident : IResident
    {
        // Свойства
        public Guid Id { get; private set; }
        public int RegistrationNumber { get; private set; }
        public string FullName { get; private set; }
        public Gender Gender { get; private set; }
        public DateTime BirthDate { get; private set; }
        public Passport Passport { get; private set; }
        public bool WorkStatus { get; set; }
        public string? Workplace { get; set; }
        public bool StudyStatus { get; set; }
        public string? StudyPlace { get; set; }
        public DateTime CheckInDate { get; private set; }

        // Конструктор
        public Resident(int registrationNumber, string fullName, Gender gender, DateTime birthDate,
                       Passport passport, DateTime checkInDate)
        {
            Id = Guid.NewGuid();
            RegistrationNumber = registrationNumber;
            FullName = fullName;
            Gender = gender;
            BirthDate = birthDate;
            Passport = passport;
            CheckInDate = checkInDate;

            // Инициализация по умолчанию
            WorkStatus = false;
            Workplace = null;
            StudyStatus = false;
            StudyPlace = null;
        }

        public string GetFullInfo()
        {
            string workInfo = WorkStatus ?
                $"Works at: {Workplace ?? "Not specified"}" :
                "Not employed";

            string studyInfo = StudyStatus ?
                $"Studies at: {StudyPlace ?? "Not specified"}" :
                "Not studying";

            return $"Resident ID: {Id}\n" +
                   $"Registration Number: {RegistrationNumber}\n" +
                   $"Full Name: {FullName}\n" +
                   $"Gender: {Gender}\n" +
                   $"Birth Date: {BirthDate:dd.MM.yyyy}\n" +
                   $"Age: {GetAge()} years\n" +
                   $"Passport: {Passport}\n" +
                   $"{workInfo}\n" +
                   $"{studyInfo}\n" +
                   $"Check-in Date: {CheckInDate:dd.MM.yyyy}\n" +
                   $"Days since check-in: {(DateTime.Now - CheckInDate).Days}";
        }

        public int GetAge()
        {
            var today = DateTime.Today;
            var age = today.Year - BirthDate.Year;

            // Если день рождения еще не наступил в этом году, вычитаем 1 год
            if (BirthDate.Date > today.AddYears(-age))
            {
                age--;
            }

            return age;
        }

        public string GetOccupantType()
        {
            return "Adult";
        }
    }
}
