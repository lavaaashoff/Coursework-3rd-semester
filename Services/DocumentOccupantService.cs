using CouseWork3Semester.Interfaces;
using CouseWork3Semester.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CouseWork3Semester.Services
{
    public class DocumentOccupantService : IDocumentOccupantService
    {
        private readonly IDocumentRegistry _documentRegistry;
        private readonly IOccupantRegistry _occupantRegistry;
        private readonly List<IDocumentOccupantLink> _links;
        private readonly IDocumentValidator _documentValidator;

        public DocumentOccupantService(
            IDocumentRegistry documentRegistry,
            IOccupantRegistry occupantRegistry,
            IDocumentValidator documentValidator)
        {
            _documentRegistry = documentRegistry ?? throw new ArgumentNullException(nameof(documentRegistry));
            _occupantRegistry = occupantRegistry ?? throw new ArgumentNullException(nameof(occupantRegistry));
            _documentValidator = documentValidator ?? throw new ArgumentNullException(nameof(documentValidator));
            _links = new List<IDocumentOccupantLink>();
        }

        public DocumentOccupantService(
            IDocumentRegistry documentRegistry,
            IOccupantRegistry occupantRegistry,
            IDocumentValidator documentValidator,
            List<IDocumentOccupantLink> initialLinks)
        {
            _documentRegistry = documentRegistry ?? throw new ArgumentNullException(nameof(documentRegistry));
            _occupantRegistry = occupantRegistry ?? throw new ArgumentNullException(nameof(occupantRegistry));
            _documentValidator = documentValidator ?? throw new ArgumentNullException(nameof(documentValidator));
            _links = initialLinks ?? new List<IDocumentOccupantLink>();
        }

        public bool AttachDocumentToOccupant(IDocument document, IRoomOccupant occupant)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document), "Документ не может быть null");

            if (occupant == null)
                throw new ArgumentNullException(nameof(occupant), "Жилец не может быть null");

            var validationResult = _documentValidator.CheckFormat(document);
            if (!validationResult.IsValid)
            {
                Console.WriteLine($"Ошибки валидации документа: {validationResult.Message}");
                return false;
            }

            bool linkExists = _links.Any(link =>
                link.DocumentId == document.Id &&
                link.OccupantId == occupant.Id);

            if (linkExists)
            {
                Console.WriteLine("Связь между документом и жильцом уже существует");
                return false;
            }

            if (!_documentRegistry.ContainsDocument(document.Id))
            {
                var existingDocumentWithSameNumber = _documentRegistry.FindDocumentByNumber(document.Number);
                if (existingDocumentWithSameNumber != null)
                {
                    Console.WriteLine("Документ с таким номером уже существует");
                    return false;
                }

                _documentRegistry.RegisterDocument(document);
            }

            if (!_occupantRegistry.ContainsOccupant(occupant.Id))
            {
                _occupantRegistry.AddOccupant(occupant);
            }

            var newLink = new DocumentOccupantLink(document.Id, occupant.Id);
            _links.Add(newLink);

            Console.WriteLine($"Документ '{document.Title}' успешно привязан к жильцу '{occupant.FullName}'");
            return true;
        }

        public List<IDocument> GetDocumentsByOccupant(IRoomOccupant occupant)
        {
            if (occupant == null)
                throw new ArgumentNullException(nameof(occupant), "Жилец не может быть null");

            return GetDocumentsByOccupantId(occupant.Id);
        }

        public List<IRoomOccupant> GetOccupantsByDocument(IDocument document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document), "Документ не может быть null");

            var occupantIds = _links
                .Where(link => link.DocumentId == document.Id)
                .Select(link => link.OccupantId)
                .Distinct()
                .ToList();

            var occupants = new List<IRoomOccupant>();
            foreach (var occupantId in occupantIds)
            {
                var occupant = _occupantRegistry.FindOccupantById(occupantId);
                if (occupant != null)
                {
                    occupants.Add(occupant);
                }
            }

            return occupants;
        }

        public List<IDocument> GetDocumentsByOccupantId(Guid occupantId)
        {
            var documentIds = _links
                .Where(link => link.OccupantId == occupantId)
                .Select(link => link.DocumentId)
                .Distinct()
                .ToList();

            var documents = new List<IDocument>();
            foreach (var documentId in documentIds)
            {
                var document = _documentRegistry.FindDocumentById(documentId);
                if (document != null)
                {
                    documents.Add(document);
                }
            }

            return documents;
        }

        public List<Guid> GetOccupantIdsByDocument(IDocument document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            return GetOccupantIdsByDocumentId(document.Id);
        }

        public List<Guid> GetOccupantIdsByDocumentId(Guid documentId)
        {
            return _links
                .Where(link => link.DocumentId == documentId)
                .Select(link => link.OccupantId)
                .Distinct()
                .ToList();
        }

        public bool IsDocumentAttachedToOccupant(Guid documentId, Guid occupantId)
        {
            return _links.Any(link =>
                link.DocumentId == documentId &&
                link.OccupantId == occupantId);
        }

        public bool DetachDocumentFromOccupant(Guid documentId, Guid occupantId)
        {
            int removedCount = _links.RemoveAll(link =>
                link.DocumentId == documentId &&
                link.OccupantId == occupantId);


            return removedCount > 0;
        }

        public List<IDocumentOccupantLink> GetAllLinks()
        {
            return new List<IDocumentOccupantLink>(_links);
        }
    }
}