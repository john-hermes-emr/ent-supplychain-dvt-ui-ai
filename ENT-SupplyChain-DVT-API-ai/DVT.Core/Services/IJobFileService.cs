using DVT.Core.Models;

namespace DVT.Core.Services
{
    public interface IJobFileService
    {
        ValueTask<JobFile> GetJobFileByIdAsync(Guid jobFileId);
        ValueTask<JobFile> GetJobFileByIdNoValidationAsync(Guid jobFileId);
        ValueTask<IEnumerable<JobFile>> GetJobFilesByJobIdAsync(Guid jobId);
        ValueTask<IEnumerable<JobFile>> GetJobFilesByJobIdNoValidationAsync(Guid jobId);
        ValueTask<IEnumerable<JobFile>> GetJobFilesByJobIdNoValidationNoTrackingAsync(Guid jobId);
        ValueTask<OperationResult> BatchUpdateJobFilesStatusAsync(List<Guid> jobFileIds, string status, string updateBy);
        ValueTask<OperationResult> UpdateJobFileValidationResultAsync(Guid jobFileId, string validationMessages, string validationStats, string status, string updateBy);
        ValueTask<OperationResult> UpdateJobFileStatusAsync(Guid jobFileId, string status, string updateBy);
        ValueTask<OperationResult> UpdateJobFilesStatusByJobIdAsync(Guid jobId, string status, string updateBy);
        ValueTask<OperationResult> DeleteJobFilesAsync(Guid jobId, string updateBy, bool isRefresh = false);
        ValueTask<OperationResult> GetJobFileValidationMessageByJobIdAndJobFileIdAsync(Guid jobId, Guid jobFileId, string userEmail);
        ValueTask<string> GetJobFileValidationFileContentsByJobFileAsync(JobFile jobFile);
        ValueTask<byte[]> GenerateJobFileErrorReportByJobIdAndJobFileIdAsync(Guid jobId, Guid jobFileId, string userEmail);
        ValueTask<byte[]> GenerateJobFileStatsReportByJobIdAndJobFileIdAsync(Guid jobId, Guid jobFileId, string userEmail);
        ValueTask<OperationResult> GetJobValidationStatsByJobIdAndJobFileIdAsync(Guid jobId, Guid jobFileId, string userEmail);

    }
}
