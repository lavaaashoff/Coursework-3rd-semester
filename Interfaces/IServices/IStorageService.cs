using CouseWork3Semester.Services;

namespace CouseWork3Semester.Interfaces
{
    public interface IStorageService
    {
        string StoragePath { get; }
        void Save(IAccountingSystem system);
        bool TryLoad(out DataState state);
    }
}