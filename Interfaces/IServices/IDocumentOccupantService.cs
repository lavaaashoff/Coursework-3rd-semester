

using CouseWork3Semester.Interfaces.IModels;

namespace CouseWork3Semester.Interfaces
{
    public interface IDocumentOccupantService
    {
        // Основные методы из UML диаграммы

        bool AttachDocumentToOccupant(IDocument document, IRoomOccupant occupant);

        List<IDocument> GetDocumentsByOccupant(IRoomOccupant occupant);

        List<IRoomOccupant> GetOccupantsByDocument(IDocument document);

        // Дополнительные полезные методы

        List<IDocument> GetDocumentsByOccupantId(Guid occupantId);

        List<Guid> GetOccupantIdsByDocument(IDocument document);

        List<Guid> GetOccupantIdsByDocumentId(Guid documentId);

        bool IsDocumentAttachedToOccupant(Guid documentId, Guid occupantId);

        bool DetachDocumentFromOccupant(Guid documentId, Guid occupantId);

        List<IDocumentOccupantLink> GetAllLinks();
    }
}