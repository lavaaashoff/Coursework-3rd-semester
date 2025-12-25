using CouseWork3Semester.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Models
{
    public class DocumentOccupantLink : IDocumentOccupantLink
    {
        public Guid Id { get; private set; }
        public Guid DocumentId { get; private set; }
        public Guid OccupantId { get; private set; } 
        public DateTime LinkCreatedDate { get; private set; }

        public DocumentOccupantLink(Guid documentId, Guid occupantId)
        {
            Id = Guid.NewGuid();
            DocumentId = documentId;
            OccupantId = occupantId;
            LinkCreatedDate = DateTime.Now;

            Validate();
        }

        public DocumentOccupantLink(Guid id, Guid documentId, Guid occupantId, DateTime linkCreatedDate)
        {
            Id = id;
            DocumentId = documentId;
            OccupantId = occupantId;
            LinkCreatedDate = linkCreatedDate;

            Validate();
        }

        private void Validate()
        {
            if (DocumentId == Guid.Empty)
                throw new ArgumentException("Document ID cannot be empty", nameof(DocumentId));

            if (OccupantId == Guid.Empty)
                throw new ArgumentException("Occupant ID cannot be empty", nameof(OccupantId));

            if (LinkCreatedDate > DateTime.Now)
                throw new ArgumentException("Link creation date cannot be in the future", nameof(LinkCreatedDate));
        }
    }
}
