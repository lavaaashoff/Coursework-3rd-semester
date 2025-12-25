using CouseWork3Semester.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Registries
{
    public class DocumentRegistry : IDocumentRegistry
    {
        public Dictionary<Guid, IDocument> Documents { get; private set; }

        public DocumentRegistry()
        {
            Documents = new Dictionary<Guid, IDocument>();
        }

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

        public void RegisterDocument(IDocument document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            if (Documents.ContainsKey(document.Id))
                throw new InvalidOperationException($"Document with ID {document.Id} is already registered");

            Documents.Add(document.Id, document);
        }

        public IDocument FindDocumentById(Guid id)
        {
            Documents.TryGetValue(id, out IDocument document);
            return document;
        }

        public IDocument FindDocumentByNumber(string number)
        {
            if (string.IsNullOrWhiteSpace(number))
                return null;

            return Documents.Values
                .FirstOrDefault(d => d.Number.Equals(number, StringComparison.OrdinalIgnoreCase));
        }


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
    }
}
