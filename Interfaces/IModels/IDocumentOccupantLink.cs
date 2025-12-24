using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Interfaces
{
    public interface IDocumentOccupantLink
    {
        // Свойства
        Guid Id { get; }
        Guid DocumentId { get; }
        Guid OccupantId { get; } // Изменили с ResidentId на OccupantId
        DateTime LinkCreatedDate { get; }

        // Методы из UML (адаптированные)
        //List<Guid> GetOccupantsByDocument(Guid documentId);
        //List<Guid> GetDocumentsByOccupant(Guid occupantId);
    }
}
