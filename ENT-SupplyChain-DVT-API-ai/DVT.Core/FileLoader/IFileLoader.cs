using DVT.Core.Models;
using DVT.Core.Services;

namespace DVT.Core.FileLoader
{
    public interface IFileLoader
    {
        Task<FileLoadResult> LoadFileAsync(FileLoadRequest fileLoadRequest, IStorageService storageService);
    }
}
