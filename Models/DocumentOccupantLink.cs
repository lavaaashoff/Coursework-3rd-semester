using CouseWork3Semester.Interfaces;
using System;
using Newtonsoft.Json;

namespace CouseWork3Semester.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class DocumentOccupantLink : IDocumentOccupantLink
    {
        [JsonProperty] public Guid Id { get; private set; }
        [JsonProperty] public Guid DocumentId { get; private set; }
        [JsonProperty] public Guid OccupantId { get; private set; }
        [JsonProperty] public DateTime LinkCreatedDate { get; private set; }

        public DocumentOccupantLink(Guid documentId, Guid occupantId)
        {
            Id = Guid.NewGuid();
            DocumentId = documentId;
            OccupantId = occupantId;
            LinkCreatedDate = DateTime.Now;
            Validate();
        }

        [JsonConstructor]
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
            if (DocumentId == Guid.Empty) throw new ArgumentException("Document ID cannot be empty", nameof(DocumentId));
            if (OccupantId == Guid.Empty) throw new ArgumentException("Occupant ID cannot be empty", nameof(OccupantId));
            if (LinkCreatedDate > DateTime.Now) throw new ArgumentException("Link creation date cannot be in the future", nameof(LinkCreatedDate));
        }
    }
}