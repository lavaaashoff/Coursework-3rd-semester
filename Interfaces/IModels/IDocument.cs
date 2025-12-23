using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Interfaces.IModels
{
    public interface IDocument
    {
        // Свойства
        Guid Id { get; }
        string Series { get; }
        string Number { get; }
        string Title { get; }
        DateTime IssueDate { get; }
        string IssuedBy { get; }
        string Comment { get; }

        // Методы
        void ChangeComment(string text);
        string GetFullData();
    }
}
