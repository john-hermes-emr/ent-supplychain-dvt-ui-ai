using DVT.Core.Models;

namespace DVT.Core.Services
{
    public interface IFileLoadService
    {
        Task<JobLoadResult> LoadFile(JobLoad jobLoad);
    }
}
