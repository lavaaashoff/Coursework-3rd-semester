using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Interfaces
{
    public interface IDocumentOccupantLink
    {
        Guid Id { get; }
        Guid DocumentId { get; }
        Guid OccupantId { get; }
        DateTime LinkCreatedDate { get; }
    }
}
