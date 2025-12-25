using CouseWork3Semester.Interfaces;

namespace CouseWork3Semester.Interfaces
{
    public interface IDocumentOccupantService
    {
        bool AttachDocumentToOccupant(IDocument document, IRoomOccupant occupant);

        List<IDocument> GetDocumentsByOccupant(IRoomOccupant occupant);

        List<IRoomOccupant> GetOccupantsByDocument(IDocument document);


        List<IDocument> GetDocumentsByOccupantId(Guid occupantId);

        List<Guid> GetOccupantIdsByDocument(IDocument document);

        List<Guid> GetOccupantIdsByDocumentId(Guid documentId);

        bool IsDocumentAttachedToOccupant(Guid documentId, Guid occupantId);

        bool DetachDocumentFromOccupant(Guid documentId, Guid occupantId);

        List<IDocumentOccupantLink> GetAllLinks();
    }
}