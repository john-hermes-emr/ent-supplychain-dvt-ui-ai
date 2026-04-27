using Azure;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using DVT.Core.Models;
using System.IO.Compression;
using System.Text;
using static DVT.Core.Constants;
using FileInfo = DVT.Core.Models.FileInfo;

namespace DVT.Core.Services
{
    public class LocalStorageService : IStorageService
    {
        private readonly string _workingFolderRootPath = @"C:\DVT\JobWorkingFolder";
        private readonly string _archiveFolderRootPath = @"C:\DVT\JobArchivesFolder";
        private readonly string _userShareRootPath = @"C:\DVT\UserShare";
        private readonly string _documentsFolderRootPath = @"C:\DVT\Documents";
        private readonly IActivityLogService _activityLogService;

        public LocalStorageService(IActivityLogService activityLogService)
        {
            _activityLogService = activityLogService;
        }
        
        private string UserSharePath(string relativePath)
            => Path.Combine(_userShareRootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));

        private string WorkingJobPath(Guid jobId)
            => Path.Combine(_workingFolderRootPath, jobId.ToString());

        private string ArchiveJobPath(Guid jobId)
            => Path.Combine(_archiveFolderRootPath, jobId.ToString());

        private static int CountDataRows(string content)
            => content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).Length - 1;

        public ValueTask<FolderList> GetFoldersByEmailAddressAsync(string emailAddress)
        {
            var root = UserSharePath(emailAddress);
            if (!Directory.Exists(root))
                throw new Exception(StardardMessages.UserDirectoryDoesNotExist);

            try
            {
                var folderList = Directory.GetDirectories(root)
                    .Select(d => ExploreDirectory(d))
                    .ToList();

                return ValueTask.FromResult(new FolderList { Folders = folderList });
            }
            catch
            {
                throw new Exception(StardardMessages.GetUserFolderError);
            }
        }

        private static Folder ExploreDirectory(string fullPath)
        {
            var folder = new Folder
            {
                Name = Path.GetFileName(fullPath),
                Children = Directory.GetDirectories(fullPath)
                    .Select(d => ExploreDirectory(d))
                    .ToList()
            };
            return folder;
        }

        public ValueTask<List<string>> GetFilesInDirectoryAsync(string folderPath)
        {
            var fullPath = UserSharePath(folderPath);
            if (!Directory.Exists(fullPath))
                throw new Exception(string.Format(StardardMessages.DirectoryDoesNotExist, folderPath));

            try
            {
                var files = GetFilesRecursive(fullPath, _userShareRootPath);
                return ValueTask.FromResult(files);
            }
            catch
            {
                throw new Exception(StardardMessages.GetUserFolderError);
            }
        }

        private static List<string> GetFilesRecursive(string fullPath, string rootPath)
        {
            var result = new List<string>();
            foreach (var file in Directory.GetFiles(fullPath))
                result.Add(ToRelativePath(file, rootPath));
            foreach (var dir in Directory.GetDirectories(fullPath))
                result.AddRange(GetFilesRecursive(dir, rootPath));
            return result;
        }

        private static string ToRelativePath(string fullPath, string rootPath)
            => fullPath.Substring(rootPath.Length).TrimStart(Path.DirectorySeparatorChar).Replace(Path.DirectorySeparatorChar, '/');

        public async ValueTask<List<FileInfo>> GetFileInfoInDirectoryAsync(string folderPath)
        {
            var fullPath = UserSharePath(folderPath);
            if (!Directory.Exists(fullPath))
                throw new Exception(string.Format(StardardMessages.DirectoryDoesNotExist, folderPath));

            try
            {
                return await CollectFileInfoAsync(fullPath, folderPath);
            }
            catch
            {
                throw new Exception(StardardMessages.GetUserFolderError);
            }
        }

        private async Task<List<FileInfo>> CollectFileInfoAsync(string fullDirPath, string relativeDirPath)
        {
            var result = new List<FileInfo>();

            foreach (var filePath in Directory.GetFiles(fullDirPath))
            {
                var fileName = Path.GetFileName(filePath);
                var content = await System.IO.File.ReadAllTextAsync(filePath);
                var info = System.IO.File.GetLastWriteTimeUtc(filePath);
                var created = System.IO.File.GetCreationTimeUtc(filePath);
                var dataCount = string.IsNullOrWhiteSpace(content) ? 0 : CountDataRows(content);

                result.Add(new FileInfo
                {
                    FileName = fileName,
                    FilePath = $"{relativeDirPath}/{fileName}",
                    FileUri = new Uri(filePath).ToString(),
                    FileContent = content,
                    RecordCount = dataCount,
                    FileCreationTimestamp = created,
                    FileLastModifiedTimestamp = info
                });
            }

            foreach (var subDir in Directory.GetDirectories(fullDirPath))
            {
                var subRelative = $"{relativeDirPath}/{Path.GetFileName(subDir)}";
                result.AddRange(await CollectFileInfoAsync(subDir, subRelative));
            }

            return result;
        }

        public async ValueTask<FileInfo> AnalyzeFileByPathAsync(string filePath)
        {
            try
            {
                var fullPath = UserSharePath(filePath);
                if (!System.IO.File.Exists(fullPath))
                    throw new Exception(string.Format(StardardMessages.FileDoesNotExist, filePath));

                var content = await System.IO.File.ReadAllTextAsync(fullPath);
                var dataCount = string.IsNullOrWhiteSpace(content) ? 0 : CountDataRows(content);

                return new FileInfo
                {
                    FileName = Path.GetFileName(fullPath),
                    FilePath = filePath,
                    FileUri = new Uri(fullPath).ToString(),
                    FileContent = content,
                    RecordCount = dataCount
                };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async ValueTask<string> GetFileContentsByPathAsync(string filePath)
        {
            try
            {
                var fullPath = UserSharePath(filePath);
                if (!System.IO.File.Exists(fullPath))
                    throw new Exception(string.Format(StardardMessages.FileDoesNotExist, filePath));

                return await System.IO.File.ReadAllTextAsync(fullPath);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async ValueTask<string> GetWorkingFileContentsAsync(Guid jobId, string fileName)
        {
            try
            {
                var fullPath = Path.Combine(WorkingJobPath(jobId), fileName);
                if (!System.IO.File.Exists(fullPath))
                    throw new Exception(string.Format(StardardMessages.FileDoesNotExist, fullPath));

                return await System.IO.File.ReadAllTextAsync(fullPath);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async ValueTask<List<FileInfo>> LoadExtractFilesAsync(Job job, IEnumerable<JobFile> newStatusFiles)
        {
            var files = new List<FileInfo>();

            try
            {
                if (Directory.Exists(_workingFolderRootPath))
                {
                    var jobDirectory = WorkingJobPath(job.JobId);

                    if (!Directory.Exists(jobDirectory))
                    {
                        jobDirectory = Directory.CreateDirectory(jobDirectory).FullName;
                    }

                    foreach (var jobFile in newStatusFiles)
                    {
                        if (string.IsNullOrEmpty(jobFile.FileName))
                            continue;

                        var sourceFilePath = Path.Combine(_userShareRootPath, jobFile.FilePath);
                        var sourceFileName = Path.GetFileName(sourceFilePath);

                        if (System.IO.File.Exists(sourceFilePath))
                        {
                            var content = await System.IO.File.ReadAllTextAsync(sourceFilePath);
                            var dataCount = string.IsNullOrWhiteSpace(content) ? 0 : CountDataRows(content);
                            var created = System.IO.File.GetCreationTimeUtc(sourceFilePath);
                            var modified = System.IO.File.GetLastWriteTimeUtc(sourceFilePath);
                            
                            files.Add(new FileInfo
                            {
                                FileName = sourceFileName,
                                RecordCount = dataCount,
                                FileCreationTimestamp = created,
                                FileLastModifiedTimestamp = modified,
                                LoadDate = DateTime.UtcNow
                            });

                            var targetPath = Path.Combine(_workingFolderRootPath, sourceFileName);
                            System.IO.File.Copy(sourceFilePath, targetPath, overwrite: true);
                        }
                        else
                        {
                            //if not found in user load folder, mark as deleted.
                            files.Add(new FileInfo
                            {
                                FileName = sourceFileName,
                                RecordCount = 0,
                                Deleted = true
                            });
                        }
                    }
                }
                else
                                   {
                    throw new Exception(StardardMessages.JobWorkingFolderDoesNotExist);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return files;
        }

        // ── create text files ─────────────────────────────────────────────────

        public async Task CreateTextFileInWorkingFolderAsync(Guid jobId, string fileName, string contents)
        {
            try
            {
                var dir = WorkingJobPath(jobId);
                if (!Directory.Exists(dir))
                    throw new Exception(string.Format(StardardMessages.DirectoryDoesNotExist, dir));

                var fullPath = Path.Combine(dir, fileName);
                await System.IO.File.WriteAllTextAsync(fullPath, contents, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating text file under job working directory: {fileName}", ex);
            }
        }

        public async Task CreateTextFileUnderUserShareAsync(string filePathAndName, string contents)
        {
            try
            {
                var fullPath = UserSharePath(filePathAndName);
                var dir = Path.GetDirectoryName(fullPath)!;
                Directory.CreateDirectory(dir);
                await System.IO.File.WriteAllTextAsync(fullPath, contents, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating text file in working folder: {filePathAndName}, Exception Message: {ex.Message}", ex);
            }
        }

        // ── delete / cleanup ──────────────────────────────────────────────────

        public Task CleanupJobWorkingDirectoryAsync(Guid jobId) => DeleteJobFilesAsync(jobId);

        public Task DeleteJobFilesAsync(Guid jobId)
        {
            try
            {
                var dir = WorkingJobPath(jobId);
                if (!Directory.Exists(dir))
                    throw new Exception(StardardMessages.JobDirectoryDoesNotExist);

                Directory.Delete(dir, recursive: true);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Task CleanupUserFolderAsync(string folderPath)
        {
            var fullPath = UserSharePath(folderPath);
            if (!Directory.Exists(fullPath))
                return Task.CompletedTask;

            foreach (var file in Directory.GetFiles(fullPath))
                System.IO.File.Delete(file);

            foreach (var dir in Directory.GetDirectories(fullPath))
                Directory.Delete(dir, recursive: true);

            return Task.CompletedTask;
        }

        // ── archive / zip ─────────────────────────────────────────────────────

        public async Task ArchiveJobfilesAsync(Job job)
        {
            try
            {
                var jobWorkingDir = WorkingJobPath(job.JobId);
                var zipPath = Path.Combine(_archiveFolderRootPath, job.JobId.ToString() + ".zip");

                Directory.CreateDirectory(_archiveFolderRootPath);

                using var zipStream = new MemoryStream();
                using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: true))
                {
                    foreach (var jobFile in job.JobFiles)
                    {
                        var fileName = Path.GetFileName(jobFile.FilePath);
                        var sourcePath = Path.Combine(jobWorkingDir, fileName);

                        if (!System.IO.File.Exists(sourcePath)) continue;

                        var entry = archive.CreateEntry(fileName);
                        using var entryStream = entry.Open();
                        using var fs = System.IO.File.OpenRead(sourcePath);
                        await fs.CopyToAsync(entryStream);
                    }
                }

                zipStream.Position = 0;
                await using var outFile = System.IO.File.Create(zipPath);
                await zipStream.CopyToAsync(outFile);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task CompressFilesToZipInWorkingFolderAsync(Guid jobId, string zipFileName, List<string> fileNamesIncluded)
        {
            try
            {
                var jobWorkingDir = WorkingJobPath(jobId);

                using var zipStream = new MemoryStream();
                using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: true))
                {
                    foreach (var fileName in fileNamesIncluded)
                    {
                        var sourcePath = Path.Combine(jobWorkingDir, fileName);
                        if (!System.IO.File.Exists(sourcePath)) continue;

                        var entry = archive.CreateEntry(fileName);
                        using var entryStream = entry.Open();
                        using var fs = System.IO.File.OpenRead(sourcePath);
                        await fs.CopyToAsync(entryStream);
                    }
                }

                var zipPath = Path.Combine(jobWorkingDir, zipFileName);
                zipStream.Position = 0;
                await using var outFile = System.IO.File.Create(zipPath);
                await zipStream.CopyToAsync(outFile);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error compressing files in working directory to zip {jobId} {zipFileName}", ex);
            }
        }

        public async Task CopyFileFromWorkingFolderToArchiveFolderAsync(Guid jobId, string fileName)
        {
            try
            {
                var sourcePath = Path.Combine(WorkingJobPath(jobId), fileName);
                var archiveJobDir = ArchiveJobPath(jobId);
                Directory.CreateDirectory(archiveJobDir);

                var targetPath = Path.Combine(archiveJobDir, fileName);

                if (System.IO.File.Exists(sourcePath))
                    System.IO.File.Copy(sourcePath, targetPath, overwrite: true);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error Copying file from working folder to Archive {jobId} {fileName}", ex);
            }
        }

        public async Task ArchiveZipLogFilesAsync(Guid jobId, List<JobFile> selectedAcceptedFiles, string outputFolder, string updateBy)
        {
            var jobWorkingDir = WorkingJobPath(jobId);
            var outputDir = UserSharePath(outputFolder);
            Directory.CreateDirectory(outputDir);

            try
            {
                var acceptedZipNames = selectedAcceptedFiles
                    .Select(x => x.FileName.Replace(".txt", ".zip", StringComparison.OrdinalIgnoreCase))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                foreach (var file in Directory.GetFiles(jobWorkingDir, "*.zip"))
                {
                    var name = Path.GetFileName(file);
                    if (!acceptedZipNames.Contains(name)) continue;

                    System.IO.File.Copy(file, Path.Combine(outputDir, name), overwrite: true);
                }
            }
            catch (Exception ex)
            {
                await _activityLogService.AddLogAsync(new ActivityLog
                {
                    LogId = Guid.NewGuid(),
                    EntityId = Guid.Empty,
                    Entity = DVTEntities.JobFile,
                    MessageType = LogMessageTypes.Error,
                    Message = StardardMessages.CreateZipFileFailed + ", Exception Message: " + ex.Message,
                    CreateBy = updateBy,
                    CreateDate = DateTime.UtcNow
                });
                throw ex;
            }
        }

        // ── log file creation ─────────────────────────────────────────────────

        public async Task<bool> CreateLogFilesAsync(Guid jobId, Guid jobFileId, string fileName, int recordCount, string logFolder, List<int> criticalRowNums, List<int> errorsRowNums, List<int> warningRowNums, DateTime updateDate, string updateBy)
        {
            var isCreated = false;

            var acceptedFileName = fileName.Replace(".txt", "_ACCEPTED.txt", StringComparison.OrdinalIgnoreCase);
            var rejectedFileName = fileName.Replace(".txt", "_REJECTED.txt", StringComparison.OrdinalIgnoreCase);
            var summaryFileName = fileName.Replace(".txt", "_SUMMARY.txt", StringComparison.OrdinalIgnoreCase);

            try
            {
                var fileContent = await GetWorkingFileContentsAsync(jobId, fileName);

                if (!criticalRowNums.Any())
                {
                    var acceptedFileContent = GenerateAcceptedFileContent(fileContent, errorsRowNums);
                    if (!string.IsNullOrWhiteSpace(acceptedFileContent))
                    {
                        await CreateTextFileUnderUserShareAsync($"{logFolder}/{acceptedFileName}", acceptedFileContent);
                        await CopyFileToWorkingFolderLocalAsync(logFolder, acceptedFileName, jobId);
                    }
                }

                var errorAndCriticalRowNums = new List<int>();

                if (criticalRowNums.Any() || errorsRowNums.Any())
                {
                    errorAndCriticalRowNums.AddRange(errorsRowNums);
                    errorAndCriticalRowNums.AddRange(criticalRowNums);
                    errorAndCriticalRowNums = errorAndCriticalRowNums.Distinct().OrderBy(x => x).ToList();

                    var rejectedFileContent = GenerateRejectedFileContent(fileContent, errorAndCriticalRowNums);
                    await CreateTextFileUnderUserShareAsync($"{logFolder}/{rejectedFileName}", rejectedFileContent);
                    await CopyFileToWorkingFolderLocalAsync(logFolder, rejectedFileName, jobId);
                }

                var rejectedRecords = errorAndCriticalRowNums.Count;
                var acceptedRecords = recordCount - rejectedRecords;

                var summaryFileContent = GenerateSummaryFileContent(acceptedRecords, rejectedRecords, updateBy, updateDate);
                await CreateTextFileUnderUserShareAsync($"{logFolder}/{summaryFileName}", summaryFileContent);
                await CopyFileToWorkingFolderLocalAsync(logFolder, summaryFileName, jobId);

                var zipFileName = fileName.Replace(".txt", ".zip", StringComparison.OrdinalIgnoreCase);
                var fileNamesInclude = new List<string> { acceptedFileName, rejectedFileName, summaryFileName };
                await CompressLogFilesToZipInWorkingFolderLocalAsync(jobId, zipFileName, fileNamesInclude, updateBy);

                isCreated = true;
            }
            catch (Exception ex)
            {
                await _activityLogService.AddLogAsync(new ActivityLog
                {
                    LogId = Guid.NewGuid(),
                    EntityId = jobFileId,
                    Entity = DVTEntities.JobFile,
                    MessageType = LogMessageTypes.Error,
                    Message = Operations.CreateLogFiles + ", Exception Message: " + ex.Message,
                    CreateBy = updateBy,
                    CreateDate = DateTime.UtcNow
                });
            }

            return isCreated;
        }

        private async Task CopyFileToWorkingFolderLocalAsync(string logFolder, string fileName, Guid jobId)
        {
            var sourcePath = UserSharePath($"{logFolder}/{fileName}");
            var targetPath = Path.Combine(WorkingJobPath(jobId), fileName);

            if (System.IO.File.Exists(sourcePath))
                System.IO.File.Copy(sourcePath, targetPath, overwrite: true);
        }

        private async Task CompressLogFilesToZipInWorkingFolderLocalAsync(Guid jobId, string zipFileName, List<string> fileNamesInclude, string updateBy)
        {
            var jobWorkingDir = WorkingJobPath(jobId);

            using var zipStream = new MemoryStream();
            using (var zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach (var fileName in fileNamesInclude)
                {
                    var sourcePath = Path.Combine(jobWorkingDir, fileName);
                    if (!System.IO.File.Exists(sourcePath)) continue;

                    var entryName = fileName.EndsWith("_ACCEPTED.txt", StringComparison.OrdinalIgnoreCase)
                        ? fileName.Replace("_ACCEPTED.txt", ".txt", StringComparison.OrdinalIgnoreCase)
                        : fileName;

                    var entry = zipArchive.CreateEntry(entryName);
                    using var entryStream = entry.Open();
                    using var fs = System.IO.File.OpenRead(sourcePath);
                    await fs.CopyToAsync(entryStream);
                }
            }

            var zipPath = Path.Combine(jobWorkingDir, zipFileName);
            zipStream.Position = 0;
            await using var outFile = System.IO.File.Create(zipPath);
            await zipStream.CopyToAsync(outFile);
        }

        // ── documents / main share ────────────────────────────────────────────

        public async Task<AzureStorageFile> GetMainShareDocsFileEntityAsync(string fileName)
        {
            try
            {
                var fullPath = Path.Combine(_documentsFolderRootPath, fileName);
                if (!System.IO.File.Exists(fullPath))
                    throw new Exception(string.Format(StardardMessages.FileDoesNotExist, fullPath));

                var bytes = await System.IO.File.ReadAllBytesAsync(fullPath);
                var contentType = GetContentType(fileName);

                return new AzureStorageFile { FileBytes = bytes, ContentType = contentType };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static string GetContentType(string fileName) =>
            Path.GetExtension(fileName).ToLowerInvariant() switch
            {
                ".pdf" => "application/pdf",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".txt" => "text/plain",
                ".csv" => "text/csv",
                ".zip" => "application/zip",
                _ => "application/octet-stream"
            };

        // ── private content generators (mirrored from StorageService) ─────────

        private static string GenerateAcceptedFileContent(string fileContent, List<int> errorsRowNums)
        {
            var rows = fileContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();

            if (!errorsRowNums.Any())
                return fileContent;

            rows = rows.Where((_, index) => !errorsRowNums.Contains(index + 1)).ToList();
            return rows.Count == 1 ? "" : string.Join(Environment.NewLine, rows);
        }

        private static string GenerateRejectedFileContent(string fileContent, List<int> errorAndCriticalRowNums)
        {
            var rows = fileContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var header = rows.FirstOrDefault();

            if (errorAndCriticalRowNums.Any())
            {
                header = "Line Number|" + header;
                rows = rows.Where((_, index) => errorAndCriticalRowNums.Contains(index + 1)).ToList();

                for (int i = 0; i < rows.Count; i++)
                    rows[i] = errorAndCriticalRowNums[i] + "|" + rows[i];

                rows.Insert(0, header);
            }

            return string.Join(Environment.NewLine, rows);
        }

        private static string GenerateSummaryFileContent(int acceptedRecords, int rejectedRecords, string userEmail, DateTime validationTimeStamp) =>
            $"Accepted Records: {acceptedRecords}{Environment.NewLine}" +
            $"Rejected Records: {rejectedRecords}{Environment.NewLine}" +
            $"Validated by: {userEmail}{Environment.NewLine}" +
            $"Validation Timestamp: {validationTimeStamp:yyyyMMdd HH:mm}";

        public Task CopyOutputFilesToSupplyChainFolderAsync(Guid jobId, List<JobFile> jobFiles)
        {
            throw new NotImplementedException();
        }
    }
}