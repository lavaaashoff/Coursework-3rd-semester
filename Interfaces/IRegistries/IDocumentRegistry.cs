using CouseWork3Semester.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Interfaces
{
    public interface IDocumentRegistry
    {
        Dictionary<Guid, IDocument> Documents { get; }

        void RegisterDocument(IDocument document);
        IDocument FindDocumentById(Guid id);
        IDocument FindDocumentByNumber(string number);

        List<IDocument> GetAllDocuments();
        bool RemoveDocument(Guid id);
        bool ContainsDocument(Guid id);
        int GetDocumentsCount();
    }
}
