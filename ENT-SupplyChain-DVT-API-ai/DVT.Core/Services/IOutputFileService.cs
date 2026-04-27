using DVT.Core.Models;

namespace DVT.Core.Services
{
    public interface IOutputFileService
    {
        ValueTask<OperationResult> CreateOutputFilesAsync(Job job, IEnumerable<JobFile> jobFiles, string outputFolder, string updateBy);
    }
}
