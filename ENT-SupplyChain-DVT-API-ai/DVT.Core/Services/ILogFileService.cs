using DVT.Core.Models;

namespace DVT.Core.Services
{
    public interface ILogFileService
    {
        ValueTask<OperationResult> CreateLogFilesAsync(Job job, List<JobFile> validatedFiles, string updateBy);
    }
}
