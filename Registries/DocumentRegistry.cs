using CouseWork3Semester.Interfaces;
using CouseWork3Semester.Interfaces.IModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Registries
{
    public class DocumentRegistry : IDocumentRegistry
    {
        // Реализация свойства из UML
        public Dictionary<Guid, IDocument> Documents { get; private set; }

        // Конструктор
        public DocumentRegistry()
        {
            Documents = new Dictionary<Guid, IDocument>();
        }

        // Конструктор с начальными документами
        public DocumentRegistry(List<IDocument> initialDocuments)
        {
            Documents = new Dictionary<Guid, IDocument>();

            if (initialDocuments != null)
            {
                foreach (var document in initialDocuments)
                {
                    RegisterDocument(document);
                }
            }
        }

        // Метод из UML: Зарегистрировать документ
        public void RegisterDocument(IDocument document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            if (Documents.ContainsKey(document.Id))
                throw new InvalidOperationException($"Document with ID {document.Id} is already registered");

            Documents.Add(document.Id, document);
        }

        // Метод из UML: Найти документ по ID
        public IDocument FindDocumentById(Guid id)
        {
            Documents.TryGetValue(id, out IDocument document);
            return document;
        }

        // Метод из UML: Найти документ по номеру
        public IDocument FindDocumentByNumber(string number)
        {
            if (string.IsNullOrWhiteSpace(number))
                return null;

            return Documents.Values
                .FirstOrDefault(d => d.Number.Equals(number, StringComparison.OrdinalIgnoreCase));
        }

        // Метод из UML: Архивировать документ
        // Делаем просто удалением, если архив не нужен
        public void ArchiveDocument(Guid id)
        {
            RemoveDocument(id);
        }

        // Дополнительные методы

        public List<IDocument> GetAllDocuments()
        {
            return Documents.Values.ToList();
        }

        public bool RemoveDocument(Guid id)
        {
            return Documents.Remove(id);
        }

        public bool ContainsDocument(Guid id)
        {
            return Documents.ContainsKey(id);
        }

        public int GetDocumentsCount()
        {
            return Documents.Count;
        }

        public bool UpdateDocumentComment(Guid documentId, string newComment)
        {
            var document = FindDocumentById(documentId);
            if (document == null)
                return false;

            document.ChangeComment(newComment);
            return true;
        }

        // Очистка реестра
        public void Clear()
        {
            Documents.Clear();
        }

        public override string ToString()
        {
            return $"Document Registry: {Documents.Count} documents";
        }
    }
}
