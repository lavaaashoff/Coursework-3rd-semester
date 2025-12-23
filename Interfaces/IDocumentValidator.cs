using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Interfaces
{
    public interface IDocumentValidator
    {
        IValidationResult CheckFormat(IDocument document);
        bool CheckValidity(IDocument document, DateTime checkDate);
        bool CheckUniqueness(IDocument document, IEnumerable<IDocument> existingDocuments);
    }
}
