using CouseWork3Semester.Interfaces.IModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CouseWork3Semester.Interfaces
{
    public interface IDocumentRegistry
    {
        // Свойство из UML
        Dictionary<Guid, IDocument> Documents { get; }

        // Методы из UML
        void RegisterDocument(IDocument document);
        IDocument FindDocumentById(Guid id);
        IDocument FindDocumentByNumber(string number);
        void ArchiveDocument(Guid id); // Оставляем, но делаем как удаление

        // Дополнительные полезные методы
        List<IDocument> GetAllDocuments();
        bool RemoveDocument(Guid id);
        bool ContainsDocument(Guid id);
        int GetDocumentsCount();
    }
}
