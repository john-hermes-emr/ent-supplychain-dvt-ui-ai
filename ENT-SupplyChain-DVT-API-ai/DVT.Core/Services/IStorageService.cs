using DVT.Core.Models;
using FileInfo = DVT.Core.Models.FileInfo;

namespace DVT.Core.Services
{
    public interface IStorageService
    {
        Task ArchiveJobfilesAsync(Job job);
        Task ArchiveZipLogFilesAsync(Guid jobId, List<JobFile> selectedAcceptedFiles, string outputFolder, string updateBy);
        ValueTask<FileInfo> AnalyzeFileByPathAsync(string filePath);
        Task CleanupJobWorkingDirectoryAsync(Guid jobId);
        Task CleanupUserFolderAsync(string folderPath);
        Task CompressFilesToZipInWorkingFolderAsync(Guid jobId, string zipFileName, List<string> fileNamesIncluded);
        Task CopyFileFromWorkingFolderToArchiveFolderAsync(Guid jobId, string fileName);
        Task CopyOutputFilesToSupplyChainFolderAsync(Guid jobId, List<JobFile> jobFiles);
        Task<bool> CreateLogFilesAsync(Guid jobId, Guid jobFileId, string fileName, int recordCount, string logFolder, List<int> criticalRowNums, List<int> errorsRowNums, List<int> warningRowNums, DateTime updateDate, string updateBy);
        Task CreateTextFileInWorkingFolderAsync(Guid jobId, string fileName, string contents);
        Task CreateTextFileUnderUserShareAsync(string filePathAndName, string contents);
        Task DeleteJobFilesAsync(Guid jobId);
        ValueTask<string> GetFileContentsByPathAsync(string filePath);
        ValueTask<List<FileInfo>> GetFileInfoInDirectoryAsync(string folderPath);
        ValueTask<List<string>> GetFilesInDirectoryAsync(string folderPath);
        ValueTask<FolderList> GetFoldersByEmailAddressAsync(string emailAddress);
        Task<AzureStorageFile> GetMainShareDocsFileEntityAsync(string fileName);
        ValueTask<string> GetWorkingFileContentsAsync(Guid jobId, string fileName);
        ValueTask<List<FileInfo>> LoadExtractFilesAsync(Job job, IEnumerable<JobFile> newStatusFiles);
    }
}
