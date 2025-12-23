using CouseWork3Semester.Interfaces.IModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Models
{
    public class DocumentOccupantLink : IDocumentOccupantLink
    {
        // Реализация свойств интерфейса
        public Guid Id { get; private set; }
        public Guid DocumentId { get; private set; }
        public Guid OccupantId { get; private set; } 
        public DateTime LinkCreatedDate { get; private set; }

        // Конструктор
        public DocumentOccupantLink(Guid documentId, Guid occupantId)
        {
            Id = Guid.NewGuid();
            DocumentId = documentId;
            OccupantId = occupantId;
            LinkCreatedDate = DateTime.Now;

            Validate();
        }

        // Конструктор для загрузки из БД
        public DocumentOccupantLink(Guid id, Guid documentId, Guid occupantId, DateTime linkCreatedDate)
        {
            Id = id;
            DocumentId = documentId;
            OccupantId = occupantId;
            LinkCreatedDate = linkCreatedDate;

            Validate();
        }

        // Валидация
        private void Validate()
        {
            if (DocumentId == Guid.Empty)
                throw new ArgumentException("Document ID cannot be empty", nameof(DocumentId));

            if (OccupantId == Guid.Empty)
                throw new ArgumentException("Occupant ID cannot be empty", nameof(OccupantId));

            if (LinkCreatedDate > DateTime.Now)
                throw new ArgumentException("Link creation date cannot be in the future", nameof(LinkCreatedDate));
        }

        // Вспомогательные методы
        public bool IsForDocument(Guid documentId)
        {
            return DocumentId == documentId;
        }

        public bool IsForOccupant(Guid occupantId)
        {
            return OccupantId == occupantId;
        }
    }
}
