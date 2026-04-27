using DVT.Core.Models;

namespace DVT.Core.Services
{
    public interface IJobService
    {
        ValueTask<OperationResult> CreateJobAsync(Job job, bool forceCreate);
        ValueTask<Job> GetUserLatestActiveJobAsync(Guid userInfoId);
        ValueTask<OperationResult> GetActiveJobResultAsync(Guid userInfoId);
        ValueTask<Job> GetJobWithJobFilesByIdAsync(Guid jobId);
        ValueTask<Job> GetJobByIdNoTrackingAsync(Guid jobId);
        ValueTask<OperationResult> UpdateJobAsync(Job job);
        ValueTask<OperationResult> LoadExtractFilesAsync(Guid jobId, string updateBy);
        ValueTask<OperationResult> GetJobStatusByIdAsync(Guid jobId);
        ValueTask<OperationResult> GetJobValidationStatsByJobIdAsync(Guid jobId, string userEmail);        
        ValueTask<OperationResult> CleanupJobWorkingDirectory(Guid jobId, string updateBy);
        ValueTask<OperationResult> UpdateJobStatusAsync(Guid jobId, string status, string updateBy);
        ValueTask<OperationResult> DeleteJobAsync(Guid jobId, string updateBy, bool isRefresh = false);
        ValueTask<OperationResult> RefreshJobAsync(Guid jobId, string updateBy);
        ValueTask<OperationResult> AcceptValidationResultAsync(Guid jobId, Guid jobFileId, string updateBy);
        ValueTask<JobStatusUpdate> GetJobAndFileStatusByJobIdAsync(Guid jobId);
        ValueTask<Job> GetJobWithJobFilesNoValidationByIdAsync(Guid jobId);
    }
}
