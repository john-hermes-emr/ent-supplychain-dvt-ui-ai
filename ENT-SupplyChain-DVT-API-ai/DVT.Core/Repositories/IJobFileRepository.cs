using DVT.Core.Models;

namespace DVT.Core.Repositories
{
    public interface IJobFileRepository : IRepository<JobFile>
    {
        ValueTask<JobFile?> GetByIdNoValidationAsync(Guid jobFileId);
        ValueTask<IEnumerable<JobFile>> GetJobFilesByJobIdAsync(Guid jobId);
        ValueTask<IEnumerable<JobFile>> GetJobFilesByJobIdNoValidationNoTrackingAsync(Guid jobId);
        ValueTask<IEnumerable<JobFile>> GetJobFilesByJobIdNoValidationAsync(Guid jobId);

        ValueTask<IEnumerable<JobFile>> GetJobFilesByJobFileIdsAsync(List<Guid> jobFileIds);
        ValueTask<IEnumerable<JobFile>> GetJobFilesByJobFileIdsNoValidationAsync(List<Guid> jobFileIds);
        ValueTask<JobFile> GetByIdNoTrackingAsync(Guid jobFileId);
    }
}
