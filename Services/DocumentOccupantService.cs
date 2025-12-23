// Services/DocumentOccupantService.cs
using CouseWork3Semester.Interfaces;
using CouseWork3Semester.Interfaces.IModels;
using CouseWork3Semester.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CouseWork3Semester.Services
{
    public class DocumentOccupantService : IDocumentOccupantService
    {
        // Зависимости как в UML (через интерфейсы) + реестр жильцов
        private readonly IDocumentRegistry _documentRegistry;
        private readonly IOccupantRegistry _occupantRegistry;
        private readonly List<IDocumentOccupantLink> _links;
        private readonly IDocumentValidator _documentValidator;

        // Конструктор
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

        // Конструктор с начальными связями
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

        // Метод из UML: Привязать документ жильцу
        public bool AttachDocumentToOccupant(IDocument document, IRoomOccupant occupant)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document), "Документ не может быть null");

            if (occupant == null)
                throw new ArgumentNullException(nameof(occupant), "Жилец не может быть null");

            // 1. Проверяем валидность документа
            var validationResult = _documentValidator.CheckFormat(document);
            if (!validationResult.IsValid)
            {
                Console.WriteLine($"Ошибки валидации документа: {validationResult.Message}");
                return false;
            }

            // 2. Проверяем, не существует ли уже такая связь
            bool linkExists = _links.Any(link =>
                link.DocumentId == document.Id &&
                link.OccupantId == occupant.Id);

            if (linkExists)
            {
                Console.WriteLine("Связь между документом и жильцом уже существует");
                return false;
            }

            // 3. Проверяем, зарегистрирован ли уже документ
            if (!_documentRegistry.ContainsDocument(document.Id))
            {
                // Документ новый - проверяем, нет ли другого документа с таким же номером
                var existingDocumentWithSameNumber = _documentRegistry.FindDocumentByNumber(document.Number);
                if (existingDocumentWithSameNumber != null)
                {
                    Console.WriteLine("Документ с таким номером уже существует");
                    return false;
                }

                // Регистрируем новый документ
                _documentRegistry.RegisterDocument(document);
            }

            // 4. Регистрируем жильца (если еще не зарегистрирован)
            if (!_occupantRegistry.ContainsOccupant(occupant.Id))
            {
                _occupantRegistry.AddOccupant(occupant);
            }

            // 5. Создаем новую связь
            var newLink = new DocumentOccupantLink(document.Id, occupant.Id);
            _links.Add(newLink);

            Console.WriteLine($"Документ '{document.Title}' успешно привязан к жильцу '{occupant.FullName}'");
            return true;
        }

        // Метод из UML: Получить документы жильца
        public List<IDocument> GetDocumentsByOccupant(IRoomOccupant occupant)
        {
            if (occupant == null)
                throw new ArgumentNullException(nameof(occupant), "Жилец не может быть null");

            return GetDocumentsByOccupantId(occupant.Id);
        }

        // Метод из UML: Получить жильцов по документу
        public List<IRoomOccupant> GetOccupantsByDocument(IDocument document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document), "Документ не может быть null");

            // Получаем ID жильцов, связанных с документом
            var occupantIds = _links
                .Where(link => link.DocumentId == document.Id)
                .Select(link => link.OccupantId)
                .Distinct()
                .ToList();

            // Получаем объекты жильцов из реестра
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

        // Вспомогательные методы

        // Получить документы жильца по его ID
        public List<IDocument> GetDocumentsByOccupantId(Guid occupantId)
        {
            // Находим все связи для этого жильца
            var documentIds = _links
                .Where(link => link.OccupantId == occupantId)
                .Select(link => link.DocumentId)
                .Distinct()
                .ToList();

            // Получаем документы по ID
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

        // Получить ID жильцов по документу
        public List<Guid> GetOccupantIdsByDocument(IDocument document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            return GetOccupantIdsByDocumentId(document.Id);
        }

        // Получить ID жильцов по ID документа
        public List<Guid> GetOccupantIdsByDocumentId(Guid documentId)
        {
            return _links
                .Where(link => link.DocumentId == documentId)
                .Select(link => link.OccupantId)
                .Distinct()
                .ToList();
        }

        // Проверить существование связи
        public bool IsDocumentAttachedToOccupant(Guid documentId, Guid occupantId)
        {
            return _links.Any(link =>
                link.DocumentId == documentId &&
                link.OccupantId == occupantId);
        }

        // Удалить связь
        public bool DetachDocumentFromOccupant(Guid documentId, Guid occupantId)
        {
            int removedCount = _links.RemoveAll(link =>
                link.DocumentId == documentId &&
                link.OccupantId == occupantId);

            // Если у жильца больше нет документов, можно решить удалять ли его из реестра
            // Пока оставим жильца в реестре, даже если у него нет документов

            return removedCount > 0;
        }

        // Получить все связи
        public List<IDocumentOccupantLink> GetAllLinks()
        {
            return new List<IDocumentOccupantLink>(_links);
        }

        // Дополнительный метод: получить все документы с их жильцами
        public Dictionary<IDocument, List<IRoomOccupant>> GetAllDocumentsWithOccupants()
        {
            var result = new Dictionary<IDocument, List<IRoomOccupant>>();

            foreach (var document in _documentRegistry.GetAllDocuments())
            {
                var occupants = GetOccupantsByDocument(document);
                result[document] = occupants;
            }

            return result;
        }

        // Дополнительный метод: получить всех жильцов с их документами
        public Dictionary<IRoomOccupant, List<IDocument>> GetAllOccupantsWithDocuments()
        {
            var result = new Dictionary<IRoomOccupant, List<IDocument>>();

            foreach (var occupant in _occupantRegistry.GetAllOccupants())
            {
                var documents = GetDocumentsByOccupant(occupant);
                result[occupant] = documents;
            }

            return result;
        }
    }
}