using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Interfaces
{
    public interface IDocument
    {
        Guid Id { get; }
        string Series { get; }
        string Number { get; }
        string Title { get; }
        DateTime IssueDate { get; }
        string IssuedBy { get; }
        string Comment { get; set; }

        void ChangeComment(string text);
        string GetFullData();
    }
}
