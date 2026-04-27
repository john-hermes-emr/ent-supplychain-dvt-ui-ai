using Azure;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Azure.Storage.Files.Shares.Specialized;
using DocumentFormat.OpenXml.Bibliography;
using DVT.Core.Models;
using Newtonsoft.Json;
using System.IO;
using System.IO.Compression;
using System.Text;
using static DVT.Core.Constants;
using FileInfo = DVT.Core.Models.FileInfo;

namespace DVT.Core.Services
{
    public class StorageService : IStorageService
    {
        private IShareClientUserShare _shareClientUserShare;
        private IShareClientMainShare _shareClientMainShare;
        private readonly IActivityLogService _activityLogService;
        private IConfigSettingService _configSettingService;

        public StorageService(IShareClientUserShare shareClientUserShare, IShareClientMainShare shareClientMainShare, IActivityLogService activityLogService, IConfigSettingService configSettingService)
        {
            _shareClientUserShare = shareClientUserShare;
            _shareClientMainShare = shareClientMainShare;
            _activityLogService = activityLogService;
            _configSettingService = configSettingService;
        }

        public async ValueTask<FileInfo> AnalyzeFileByPathAsync(string filePath)
        {
            try
            {
                var directoryPath = Path.GetDirectoryName(filePath);
                ShareDirectoryClient directoryClient = _shareClientUserShare.GetDirectoryClient(directoryPath);

                if (!await directoryClient.ExistsAsync())
                {
                    throw new Exception(string.Format(StardardMessages.DirectoryDoesNotExist, filePath));
                }

                var fileName = Path.GetFileName(filePath);

                ShareFileClient shareFileClient = directoryClient.GetFileClient(fileName);

                if (!await shareFileClient.ExistsAsync())
                {
                    throw new Exception(string.Format(StardardMessages.FileDoesNotExist, filePath));
                }

                string content;
                int dataCount = 0;

                using (Stream download = await shareFileClient.OpenReadAsync())
                {
                    using (StreamReader reader = new StreamReader(download))
                    {
                        content = await reader.ReadToEndAsync();
                    }
                }

                if (!string.IsNullOrWhiteSpace(content))
                {
                    dataCount = content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).Length - 1;
                }

                return new FileInfo
                {
                    FileName = fileName,
                    FilePath = shareFileClient.Path,
                    FileUri = shareFileClient.Uri.ToString(),
                    FileContent = content,
                    RecordCount = dataCount
                };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task ArchiveJobfilesAsync(Job job)
        {
            try
            {
                ShareDirectoryClient directory = _shareClientMainShare.GetDirectoryClient(WellKnownStorageAccountDirectoryNames.JobWorkingFolder + "/" + job.JobId.ToString());

                var files = job.JobFiles;
                var filePath = "";
                var fileName = "";
                var content = "";
                ShareFileClient shareFileClient;
                Stream download;
                StreamReader reader;
                StreamWriter writer;
                ZipArchiveEntry entry;

                using (MemoryStream zipStream = new MemoryStream())
                {
                    using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
                    {
                        foreach (var item in files)
                        {
                            filePath = item.FilePath;
                            fileName = Path.GetFileName(filePath);
                            shareFileClient = directory.GetFileClient(fileName);
                            if (await shareFileClient.ExistsAsync())
                            {
                                using (download = await shareFileClient.OpenReadAsync())
                                {
                                    using (reader = new StreamReader(download))
                                    {
                                        content = await reader.ReadToEndAsync();
                                    }
                                }

                                entry = archive.CreateEntry(fileName);
                                using (writer = new StreamWriter(entry.Open()))
                                {
                                    writer.Write(content);
                                }
                            }
                        }
                    }

                    zipStream.Position = 0;

                    ShareDirectoryClient jobArchiveDirectory = _shareClientMainShare.GetDirectoryClient(WellKnownStorageAccountDirectoryNames.JobArchives);

                    ShareFileClient zipFile = await jobArchiveDirectory.CreateFileAsync(job.JobId.ToString() + ".zip", zipStream.Length);

                    await zipFile.UploadAsync(zipStream);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task ArchiveZipLogFilesAsync(Guid jobId, List<JobFile> selectedAcceptedFiles, string outputFolder, string updateBy)
        {
            ShareDirectoryClient jobWorkingDirectoryClient = _shareClientMainShare.GetDirectoryClient(WellKnownStorageAccountDirectoryNames.JobWorkingFolder + "/" + jobId.ToString());

            ShareDirectoryClient outputDirectoryClient = _shareClientUserShare.GetDirectoryClient(outputFolder);

            await outputDirectoryClient.CreateIfNotExistsAsync();

            var fileAndDirectories = jobWorkingDirectoryClient.GetFilesAndDirectoriesAsync();

            try
            {
                //Files in working directory.
                await foreach (ShareFileItem fileItem in fileAndDirectories)
                {
                    if (!fileItem.IsDirectory) // Only process files
                    {
                        if (!fileItem.Name.EndsWith(".zip"))
                        {
                            continue;
                        }

                        if (!selectedAcceptedFiles.Select(x => x.FileName.Replace(".txt", ".zip", StringComparison.OrdinalIgnoreCase)).Contains(fileItem.Name))
                        {
                            continue;
                        }

                        var fileClient = jobWorkingDirectoryClient.GetFileClient(fileItem.Name);

                        ShareFileClient archiveFileClient = outputDirectoryClient.GetFileClient(fileItem.Name);

                        await archiveFileClient.StartCopyAsync(fileClient.Uri);

                        //Delete the original file after copy to archive folder.
                        //await fileClient.DeleteAsync();
                    }
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

        public async Task CleanupJobWorkingDirectoryAsync(Guid jobId)
        {
            await DeleteJobFilesAsync(jobId);
        }

        public async Task CleanupUserFolderAsync(string folderPath)
        {
            ShareDirectoryClient directory = _shareClientUserShare.GetDirectoryClient(folderPath);
            if (await directory.ExistsAsync())
            {
                await foreach (ShareFileItem fileItem in directory.GetFilesAndDirectoriesAsync())
                {
                    if (fileItem.IsDirectory)
                    {
                        // Recursively delete subdirectory
                        ShareDirectoryClient subDirectoryClient = directory.GetSubdirectoryClient(fileItem.Name);
                        await DeleteDirectoryRecursive(subDirectoryClient);
                    }
                    else
                    {
                        // Delete file
                        ShareFileClient fileClient = directory.GetFileClient(fileItem.Name);
                        await fileClient.DeleteAsync();
                    }
                }
            }
            //await directory.DeleteAsync();
        }

        public async Task CompressFilesToZipInWorkingFolderAsync(Guid jobId, string zipFileName, List<string> fileNamesIncluded)
        {
            try
            {
                ShareDirectoryClient jobWorkingDirectory = _shareClientMainShare.GetDirectoryClient(WellKnownStorageAccountDirectoryNames.JobWorkingFolder + "/" + jobId.ToString());
                using (MemoryStream zipStream = new MemoryStream())
                {
                    using (ZipArchive zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
                    {
                        foreach (var fileName in fileNamesIncluded)
                        {
                            ShareFileClient fileClient = jobWorkingDirectory.GetFileClient(fileName);

                            if (await fileClient.ExistsAsync())
                            {
                                Stream download = await fileClient.OpenReadAsync();
                                ZipArchiveEntry zipEntry = zipArchive.CreateEntry(fileName);
                                using (Stream entryStream = zipEntry.Open())
                                {
                                    await download.CopyToAsync(entryStream);
                                }
                            }
                        }
                    }

                    ShareFileClient zipFileClient = jobWorkingDirectory.GetFileClient(zipFileName);
                    await zipFileClient.CreateAsync(zipStream.Length);
                    zipStream.Position = 0;
                    await zipFileClient.UploadAsync(zipStream);
                }
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
                ShareDirectoryClient jobWorkingDirectory = _shareClientMainShare.GetDirectoryClient(WellKnownStorageAccountDirectoryNames.JobWorkingFolder + "/" + jobId.ToString());
                ShareFileClient sourceFileClient = jobWorkingDirectory.GetFileClient(fileName);

                ShareDirectoryClient jobArchivesDirectory = _shareClientMainShare.GetDirectoryClient(WellKnownStorageAccountDirectoryNames.JobArchives);

                //Check if the sub-directory for this particular job exists
                ShareDirectoryClient jobArchiveDir = jobArchivesDirectory.GetSubdirectoryClient(jobId.ToString());

                if (!await jobArchiveDir.ExistsAsync())
                {
                    jobArchiveDir = await jobArchivesDirectory.CreateSubdirectoryAsync(jobId.ToString());
                }

                ShareFileClient targetFileClient = jobArchiveDir.GetFileClient(fileName);

                if (await sourceFileClient.ExistsAsync())
                {
                    await targetFileClient.StartCopyAsync(sourceFileClient.Uri);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error Copying file from working folder to Archive {jobId} {fileName}", ex);
            }
        }

        public async Task CopyOutputFilesToSupplyChainFolderAsync(Guid jobId, List<JobFile> jobFiles)
        {
            if(jobId == Guid.Empty || jobFiles == null || !jobFiles.Any())
                throw new ArgumentException("Invalid jobId or jobFiles.");

            //Build the list of files to copy based on the job files from the original file names                
            var origFileNames = jobFiles.Select(x => Path.GetFileNameWithoutExtension(x.FileName) + ".zip").ToList();

            //Get the target folder from settings
            var targetFolder = await _configSettingService.GetSettingByModuleAndNameAsync(
                WellKnownConfigSettingModules.MainShareFolderPaths, WellKnownPathNames.SupplyChainTargetFolder);

            if (targetFolder == null)
                throw new Exception($"Config setting for {WellKnownPathNames.SupplyChainTargetFolder} is not found.");

            await CopyFilesWithinMainShare(WellKnownStorageAccountDirectoryNames.JobWorkingFolder + "/" + jobId.ToString()
                , targetFolder.Value, origFileNames);
        }

        public async Task<bool> CreateLogFilesAsync(Guid jobId, Guid jobFileId, string fileName, int recordCount, string logFolder, List<int> criticalRowNums, List<int> errorsRowNums, List<int> warningRowNums, DateTime updateDate, string updateBy)
        {
            var isCreated = false;
            ShareDirectoryClient directoryClient = _shareClientUserShare.GetDirectoryClient(logFolder);

            //check and create if folder not exist.
            await DirectoryExistsAsync(logFolder, directoryClient, true);

            var acceptedFileName = fileName.Replace(".txt", "_ACCEPTED.txt", StringComparison.OrdinalIgnoreCase);

            var rejectedFileName = fileName.Replace(".txt", "_REJECTED.txt", StringComparison.OrdinalIgnoreCase);

            var summaryFileName = fileName.Replace(".txt", "_SUMMARY.txt", StringComparison.OrdinalIgnoreCase);

            try
            {
                var logDirectory = _shareClientUserShare.GetDirectoryClient(logFolder);

                var fileContent = await GetWorkingFileContentsAsync(jobId, fileName);
                //No critical, create accepted file, include validated and warning records, no errors and critical.
                if (!criticalRowNums.Any())
                {
                    var acceptedFileContent = GenerateAcceptedFileContent(fileContent, errorsRowNums);
                    if (!string.IsNullOrWhiteSpace(acceptedFileContent))
                    {
                        await CreateTextFileUnderUserShareAsync(logFolder + "/" + acceptedFileName, acceptedFileContent);
                        //copy to working folder
                        await CopyFileToWorkingFolderAsync(logDirectory.GetFileClient(acceptedFileName).Path, jobId, acceptedFileName);
                    }
                }

                var errorAndCriticalRowNums = new List<int>();

                //Have critical or errors, create rejected file
                if (criticalRowNums.Any() || errorsRowNums.Any())
                {
                    errorAndCriticalRowNums.AddRange(errorsRowNums);
                    errorAndCriticalRowNums.AddRange(criticalRowNums);
                    errorAndCriticalRowNums = errorAndCriticalRowNums.Distinct().OrderBy(x => x).ToList();

                    var rejectedFileContent = GenerateRejectedFileContent(fileContent, errorAndCriticalRowNums);
                    await CreateTextFileUnderUserShareAsync(logFolder + "/" + rejectedFileName, rejectedFileContent);
                    await CopyFileToWorkingFolderAsync(logDirectory.GetFileClient(rejectedFileName).Path, jobId, rejectedFileName);
                }

                var rejectedRecords = errorAndCriticalRowNums.Count;
                var acceptedRecords = recordCount - rejectedRecords;

                //Create summary file
                var summaryFileContent = GenerateSummaryFileContent(acceptedRecords, rejectedRecords, updateBy, updateDate);
                await CreateTextFileUnderUserShareAsync(logFolder + "/" + summaryFileName, summaryFileContent);
                await CopyFileToWorkingFolderAsync(logDirectory.GetFileClient(summaryFileName).Path, jobId, summaryFileName);

                var zipFileName = fileName.Replace(".txt", ".zip", StringComparison.OrdinalIgnoreCase);
                var fileNamesInclude = new List<string> { acceptedFileName, rejectedFileName, summaryFileName };

                //compress these files into zip in working folder
                await CompressLogFilesToZipInWorkingFolderAsync(jobId, zipFileName, fileNamesInclude, updateBy);

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

        public async Task CreateTextFileInWorkingFolderAsync(Guid jobId, string fileName, string contents)
        {
            try
            {
                ShareDirectoryClient directoryClient = _shareClientMainShare.GetDirectoryClient(WellKnownStorageAccountDirectoryNames.JobWorkingFolder + "/" + jobId);
                if (!await directoryClient.ExistsAsync())
                {
                    throw new Exception(string.Format(StardardMessages.DirectoryDoesNotExist, directoryClient.Path));
                }

                //Create the file, if it exists, delete it first.
                ShareFileClient file = directoryClient.GetFileClient(fileName);

                if (file.Exists())
                {
                    await file.DeleteAsync();
                }

                int uploadLimit = 4 * 1024 * 1024; // 4 MB
                long index = 0;
                byte[] buffer = new byte[uploadLimit];
                int bytesRead;

                using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(contents)))
                {
                    await file.CreateAsync(stream.Length);
                    //Using the while part instead of UploadRangeAsync to upload large file in chunks.
                    //await file.UploadRangeAsync(new HttpRange(0, stream.Length), stream);

                    // Read the stream in chunks
                    while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        // Create a MemoryStream for the current chunk
                        using (MemoryStream ms = new MemoryStream(buffer, 0, bytesRead))
                        {
                            // Upload the chunk using UploadRangeAsync
                            // Specify the range (index and length) and the stream for the chunk
                            await file.UploadRangeAsync(ShareFileRangeWriteType.Update,
                                new HttpRange(index, ms.Length), ms);

                            // Increment the index to account for bytes already written
                            index += ms.Length;
                        }
                    }
                }
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
                var directoryPath = Path.GetDirectoryName(filePathAndName);
                var fileName = Path.GetFileName(filePathAndName);

                ShareDirectoryClient directory = _shareClientUserShare.GetDirectoryClient(directoryPath);

                await DirectoryExistsAsync(directoryPath, directory, true);

                ShareFileClient file = directory.GetFileClient(fileName);

                if (file.Exists())
                {
                    await file.DeleteAsync();
                }

                int uploadLimit = 4 * 1024 * 1024; // 4 MB
                long index = 0;
                byte[] buffer = new byte[uploadLimit];
                int bytesRead;

                using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(contents)))
                {
                    await file.CreateAsync(stream.Length);
                    //Using the while part instead of UploadRangeAsync to upload large file in chunks.
                    //await file.UploadRangeAsync(new HttpRange(0, stream.Length), stream);

                    // Read the stream in chunks
                    while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        // Create a MemoryStream for the current chunk
                        using (MemoryStream ms = new MemoryStream(buffer, 0, bytesRead))
                        {
                            // Upload the chunk using UploadRangeAsync
                            // Specify the range (index and length) and the stream for the chunk
                            await file.UploadRangeAsync(ShareFileRangeWriteType.Update,
                                new HttpRange(index, ms.Length), ms);

                            // Increment the index to account for bytes already written
                            index += ms.Length;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating text file in working folder: {filePathAndName}, Exception Message: {ex.Message}", ex);
            }
        }

        public async Task DeleteJobFilesAsync(Guid jobId)
        {
            try
            {
                ShareDirectoryClient directory = _shareClientMainShare.GetDirectoryClient(WellKnownStorageAccountDirectoryNames.JobWorkingFolder + "/" + jobId.ToString());
                if (await directory.ExistsAsync())
                {
                    await DeleteDirectoryRecursive(directory);
                }
                else
                {
                    throw new Exception(StardardMessages.JobDirectoryDoesNotExist);
                }
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
                var directoryPath = Path.GetDirectoryName(filePath);
                ShareDirectoryClient directoryClient = _shareClientUserShare.GetDirectoryClient(directoryPath);
                if (!await directoryClient.ExistsAsync())
                {
                    throw new Exception(string.Format(StardardMessages.DirectoryDoesNotExist, filePath));
                }

                var fileName = Path.GetFileName(filePath);
                ShareFileClient shareFileClient = directoryClient.GetFileClient(fileName);
                if (!await shareFileClient.ExistsAsync())
                {
                    throw new Exception(string.Format(StardardMessages.FileDoesNotExist, filePath));
                }

                string content;
                using (Stream download = await shareFileClient.OpenReadAsync())
                {
                    using (StreamReader reader = new StreamReader(download))
                    {
                        content = await reader.ReadToEndAsync();
                    }
                }
                return content;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async ValueTask<List<FileInfo>> GetFileInfoInDirectoryAsync(string folderPath)
        {
            ShareDirectoryClient directoryClient = _shareClientUserShare.GetDirectoryClient(folderPath);

            if (!await directoryClient.ExistsAsync())
            {
                throw new Exception(string.Format(StardardMessages.DirectoryDoesNotExist, folderPath));
            }

            var filesAndDirectories = directoryClient.GetFilesAndDirectoriesAsync();

            if (filesAndDirectories == null)
            {
                throw new Exception(string.Format(StardardMessages.NoFilesFoundInDirectory, folderPath));
            }

            var fileList = new List<FileInfo>();

            try
            {
                string fileName;
                string content;
                ShareFileClient shareFileClient;
                int dataCount = 0;
                DateTime? fileCreationDate;
                DateTime? fileLastModifiedDate;
                await foreach (ShareFileItem file in filesAndDirectories)
                {
                    if (file is ShareFileItem fileItem)
                    {
                        if (fileItem.IsDirectory)
                        {
                            var files = await ExploreFileInfo($"{folderPath}/{file.Name}");
                            fileList.AddRange(files);
                        }
                        else
                        {
                            content = "";
                            dataCount = 0;
                            fileCreationDate = null;
                            fileLastModifiedDate = null;

                            fileName = Path.GetFileName(fileItem.Name);

                            shareFileClient = directoryClient.GetFileClient(fileName);

                            if (!await shareFileClient.ExistsAsync())
                            {
                                throw new Exception(string.Format(StardardMessages.FileDoesNotExist, fileItem.Name));
                            }

                            using (Stream download = await shareFileClient.OpenReadAsync())
                            {
                                using (StreamReader reader = new StreamReader(download))
                                {
                                    content = await reader.ReadToEndAsync();
                                }
                            }

                            if (!string.IsNullOrWhiteSpace(content))
                            {
                                dataCount = content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).Length - 1;
                            }

                            var properties = await shareFileClient.GetPropertiesAsync();

                            fileCreationDate = GetFileDateInfo(properties);
                            fileLastModifiedDate = GetFileDateInfo(properties, false);

                            fileList.Add(new FileInfo
                            {
                                FileName = fileItem.Name,
                                FilePath = $"{folderPath}/{fileItem.Name}",
                                FileUri = directoryClient.GetFileClient(fileItem.Name).Uri.ToString(),
                                FileContent = content,
                                RecordCount = dataCount,
                                FileCreationTimestamp = fileCreationDate,
                                FileLastModifiedTimestamp = fileLastModifiedDate
                            });
                        }
                    }
                }

                string json = JsonConvert.SerializeObject(fileList, Formatting.Indented);

                return fileList;
            }
            catch (Exception)
            {
                throw new Exception(StardardMessages.GetUserFolderError);
            }
        }

        public async ValueTask<List<string>> GetFilesInDirectoryAsync(string folderPath)
        {
            ShareDirectoryClient directoryClient = _shareClientUserShare.GetDirectoryClient(folderPath);

            if (!await directoryClient.ExistsAsync())
            {
                throw new Exception(string.Format(StardardMessages.DirectoryDoesNotExist, folderPath));
            }

            var filesAndDirectories = directoryClient.GetFilesAndDirectoriesAsync();

            if (filesAndDirectories == null)
            {
                throw new Exception(string.Format(StardardMessages.NoFilesFoundInDirectory, folderPath));
            }

            var fileList = new List<string>();
            try
            {
                ShareFileClient fileClient;

                await foreach (ShareFileItem file in filesAndDirectories)
                {
                    if (file is ShareFileItem fileItem)
                    {
                        if (fileItem.IsDirectory)
                        {
                            var files = await ExploreFileAsync($"{folderPath}/{file.Name}");
                            fileList.AddRange(files);
                        }
                        else
                        {
                            fileList.Add($"{folderPath}/{file.Name}");
                        }
                    }
                }

                string json = JsonConvert.SerializeObject(fileList, Formatting.Indented);

                return fileList;
            }
            catch (Exception)
            {
                throw new Exception(StardardMessages.GetUserFolderError);
            }
        }

        public async ValueTask<FolderList> GetFoldersByEmailAddressAsync(string emailAddress)
        {
            ShareDirectoryClient directory = _shareClientUserShare.GetDirectoryClient(emailAddress);
            if (!await directory.ExistsAsync())
            {
                throw new Exception(StardardMessages.UserDirectoryDoesNotExist);
            }

            var filesAndDirectories = directory.GetFilesAndDirectoriesAsync();
            var folders = new FolderList();
            var folderList = new List<Folder>();
            try
            {
                var currentFolderName = "";
                await foreach (ShareFileItem item in filesAndDirectories)
                {
                    if (item is ShareFileItem fileItem && fileItem.IsDirectory)
                    {
                        currentFolderName = item.Name;
                        var folder = await ExploreDirectory(emailAddress + "/" + item.Name, currentFolderName);
                        folderList.Add(folder);
                    }
                }

                folders.Folders = folderList;

                return folders;
            }
            catch (Exception)
            {
                throw new Exception(StardardMessages.GetUserFolderError);
            }
        }

        /// <summary>
        /// this just for get AzureStorageFile from main-share/Documents.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async Task<AzureStorageFile> GetMainShareDocsFileEntityAsync(string fileName)
        {
            try
            {
                ShareDirectoryClient directoryClient = _shareClientMainShare.GetDirectoryClient(WellKnownStorageAccountDirectoryNames.Documents);
                if (!await directoryClient.ExistsAsync())
                {
                    throw new Exception(string.Format(StardardMessages.DirectoryDoesNotExist, directoryClient.Path));
                }

                ShareFileClient shareFileClient = directoryClient.GetFileClient(fileName);
                if (!await shareFileClient.ExistsAsync())
                {
                    throw new Exception(string.Format(StardardMessages.FileDoesNotExist, shareFileClient.Path));
                }

                var stream = await shareFileClient.OpenReadAsync();

                using (MemoryStream ms = new MemoryStream())
                {
                    await stream.CopyToAsync(ms);
                    return new AzureStorageFile
                    {
                        FileBytes = ms.ToArray(),
                        ContentType = shareFileClient.GetProperties().Value.ContentType
                    };
                }
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
                ShareDirectoryClient directoryClient = _shareClientMainShare.GetDirectoryClient(WellKnownStorageAccountDirectoryNames.JobWorkingFolder + "/" + jobId);
                if (!await directoryClient.ExistsAsync())
                {
                    throw new Exception(string.Format(StardardMessages.DirectoryDoesNotExist, directoryClient.Path));
                }

                ShareFileClient shareFileClient = directoryClient.GetFileClient(fileName);
                if (!await shareFileClient.ExistsAsync())
                {
                    throw new Exception(string.Format(StardardMessages.FileDoesNotExist, shareFileClient.Path));
                }

                string content;
                using (Stream download = await shareFileClient.OpenReadAsync())
                {
                    using (StreamReader reader = new StreamReader(download))
                    {
                        content = await reader.ReadToEndAsync();
                    }
                }
                return content;
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
                ShareDirectoryClient directory = _shareClientMainShare.GetDirectoryClient(WellKnownStorageAccountDirectoryNames.JobWorkingFolder);
                if (await directory.ExistsAsync())
                {
                    ShareDirectoryClient jobDirectory = directory.GetSubdirectoryClient(job.JobId.ToString());

                    if (!await jobDirectory.ExistsAsync())
                    {
                        jobDirectory = await directory.CreateSubdirectoryAsync(job.JobId.ToString());
                    }

                    string sourceFilePath = "";
                    string sourceFileDirectory = "";
                    string sourceFileName = "";

                    ShareDirectoryClient sourceDirectoryClient;
                    ShareFileClient sourceFileClient;
                    ShareFileClient targetFileClient;
                    Response<ShareFileProperties> properties;
                    var content = "";
                    var dataCount = 0;
                    DateTime? fileCreationDate;
                    DateTime? fileLastModifiedDate;
                    //copy file to job working folder
                    foreach (var jobFile in newStatusFiles)
                    {
                        //var jobStatusUpdate = new JobStatusUpdate(job.JobId);

                        content = "";
                        dataCount = 0;
                        properties = null;
                        fileCreationDate = null;
                        fileLastModifiedDate = null;

                        //!!!Notice here!!! Must connect paths using /. Unless the path cannot find after deploy on dev.
                        //The file path likes: kaka.li@emerson.com/a1\vla_18_inv_o.txt
                        sourceFilePath = jobFile.FilePath.Replace("\\", "/");
                        sourceFileDirectory = Path.GetDirectoryName(sourceFilePath);

                        if (string.IsNullOrWhiteSpace(sourceFileDirectory))
                        {
                            continue;
                        }

                        sourceFileName = Path.GetFileName(sourceFilePath);

                        sourceDirectoryClient = _shareClientUserShare.GetDirectoryClient(sourceFileDirectory);

                        sourceFileClient = sourceDirectoryClient.GetFileClient(sourceFileName);

                        if (await sourceFileClient.ExistsAsync())
                        {
                            properties = await sourceFileClient.GetPropertiesAsync();

                            targetFileClient = jobDirectory.GetFileClient(sourceFileName);

                            fileCreationDate = GetFileDateInfo(properties);
                            fileLastModifiedDate = GetFileDateInfo(properties, false);

                            //19441914 [QA Bug] - all screen for record counts should be match
                            //read content and get row count.
                            using (Stream download = await sourceFileClient.OpenReadAsync())
                            {
                                using (StreamReader reader = new StreamReader(download))
                                {
                                    content = await reader.ReadToEndAsync();
                                }
                            }

                            if (!string.IsNullOrWhiteSpace(content))
                            {
                                dataCount = content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).Length - 1;
                            }

                            files.Add(new FileInfo
                            {
                                FileName = sourceFileName,
                                RecordCount = dataCount,
                                FileCreationTimestamp = fileCreationDate,
                                FileLastModifiedTimestamp = fileLastModifiedDate,
                                LoadDate = DateTime.UtcNow
                            });

                            await targetFileClient.StartCopyAsync(sourceFileClient.Uri);

                            //jobStatusUpdate.AddFileStatus(jobFile.JobFileId, WellKnownFileStatuses.Uploaded);

                            //await _realtimeStatusReportService.SendJobStatusUpdate(jobStatusUpdate);
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

        private async Task CompressLogFilesToZipInWorkingFolderAsync(Guid jobId, string zipFileName, List<string> fileNamesInclude, string updateBy)
        {
            ShareDirectoryClient jobWorkingDirectory = _shareClientMainShare.GetDirectoryClient(WellKnownStorageAccountDirectoryNames.JobWorkingFolder + "/" + jobId.ToString());
            using (MemoryStream zipStream = new MemoryStream())
            {
                using (ZipArchive zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
                {
                    foreach (var fileName in fileNamesInclude)
                    {
                        ShareFileClient fileClient = jobWorkingDirectory.GetFileClient(fileName);

                        if (await fileClient.ExistsAsync())
                        {
                            Stream download = await fileClient.OpenReadAsync();

                            var txtFileName = fileName;

                            if (txtFileName.EndsWith("_ACCEPTED.txt", StringComparison.OrdinalIgnoreCase))
                            {
                                txtFileName = txtFileName.Replace("_ACCEPTED.txt", ".txt", StringComparison.OrdinalIgnoreCase);
                            }
                            ZipArchiveEntry zipEntry = zipArchive.CreateEntry(txtFileName);
                            using (Stream entryStream = zipEntry.Open())
                            {
                                await download.CopyToAsync(entryStream);
                            }
                        }
                    }
                }

                ShareFileClient zipFileClient = jobWorkingDirectory.GetFileClient(zipFileName);
                await zipFileClient.CreateAsync(zipStream.Length);
                zipStream.Position = 0;
                await zipFileClient.UploadAsync(zipStream);
            }
        }

        private async Task CopyFileToWorkingFolderAsync(string sourceFilePath, Guid jobId, string fileName)
        {
            var directoryPath = Path.GetDirectoryName(sourceFilePath);
            ShareDirectoryClient sourceDirectoryClient = _shareClientUserShare.GetDirectoryClient(directoryPath);
            ShareFileClient sourceFileClient = sourceDirectoryClient.GetFileClient(fileName);
            ShareDirectoryClient jobWorkingDirectory = _shareClientMainShare.GetDirectoryClient(WellKnownStorageAccountDirectoryNames.JobWorkingFolder + "/" + jobId.ToString());
            ShareFileClient targetFileClient = jobWorkingDirectory.GetFileClient(fileName);
            if (await sourceFileClient.ExistsAsync())
            {
                await targetFileClient.StartCopyAsync(sourceFileClient.Uri);
            }
        }

        private async Task CopyFilesWithinMainShare(string sourceDirectory, string targetDirectory, List<string> fileList)
        {
            try
            {
                ShareDirectoryClient sourceDirectoryClient = _shareClientMainShare.GetDirectoryClient(sourceDirectory);
                ShareDirectoryClient targetDirectoryClient = _shareClientMainShare.GetDirectoryClient(targetDirectory);

                if (!await targetDirectoryClient.ExistsAsync())
                    throw new DirectoryNotFoundException($"Target directory {targetDirectory} does not exist in main share.");

                foreach (var fileName in fileList)
                {
                    ShareFileClient sourceFileClient = sourceDirectoryClient.GetFileClient(fileName);
                    ShareFileClient targetFileClient = targetDirectoryClient.GetFileClient(fileName);
                    if (await sourceFileClient.ExistsAsync())
                    {
                        await targetFileClient.StartCopyAsync(sourceFileClient.Uri);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error copying files within main share from {sourceDirectory} to {targetDirectory}", ex);
            }
        }

        private async Task DeleteDirectoryRecursive(ShareDirectoryClient directory)
        {
            await foreach (ShareFileItem fileItem in directory.GetFilesAndDirectoriesAsync())
            {
                if (fileItem.IsDirectory)
                {
                    // Recursively delete subdirectory
                    ShareDirectoryClient subDirectoryClient = directory.GetSubdirectoryClient(fileItem.Name);
                    await DeleteDirectoryRecursive(subDirectoryClient);
                }
                else
                {
                    // Delete file
                    ShareFileClient fileClient = directory.GetFileClient(fileItem.Name);
                    await fileClient.DeleteAsync();
                }
            }

            await directory.DeleteIfExistsAsync();
        }

        private async Task<bool> DirectoryExistsAsync(string directory, ShareDirectoryClient directoryClient, bool isCreateIfNotFound = true)
        {
            try
            {
                var isExists = false;

                if (await directoryClient.ExistsAsync())
                {
                    isExists = true;
                }
                else
                {
                    var parentDirectory = Path.GetDirectoryName(directory);
                    var currentDirectory = Path.GetFileName(directory);
                    ShareDirectoryClient parentDirectoryClient = _shareClientUserShare.GetDirectoryClient(parentDirectory);

                    if (await parentDirectoryClient.ExistsAsync())
                    {
                        isExists = true;

                        if (isCreateIfNotFound)
                        {
                            ShareDirectoryClient currentDirectoryClient = parentDirectoryClient.GetSubdirectoryClient(currentDirectory);

                            if (!await currentDirectoryClient.ExistsAsync() && isCreateIfNotFound)
                            {
                                await currentDirectoryClient.CreateAsync();
                                isExists = true;
                            }
                            else
                            {
                                isExists = true;
                            }
                        }
                    }
                    else
                    {
                        isExists = await DirectoryExistsAsync(parentDirectory, parentDirectoryClient, isCreateIfNotFound);
                    }

                    if (!await directoryClient.ExistsAsync() && isCreateIfNotFound)
                    {
                        await directoryClient.CreateAsync();
                        isExists = true;
                    }
                }

                return isExists;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private async Task<Folder> ExploreDirectory(string directoryPath, string parentFolder)
        {
            ShareDirectoryClient directoryClient = _shareClientUserShare.GetDirectoryClient(directoryPath);

            if (!await directoryClient.ExistsAsync())
            {
                throw new Exception(string.Format(StardardMessages.DirectoryDoesNotExist, directoryPath));
            }

            var folder = new Folder();
            folder.Name = parentFolder;
            var children = new List<Folder>();

            var directories = directoryClient.GetFilesAndDirectoriesAsync();

            await foreach (ShareFileItem item in directories)
            {
                if (item is ShareFileItem fileItem && fileItem.IsDirectory)
                {
                    {
                        children.Add(await ExploreDirectory($"{directoryPath}/{item.Name}", item.Name));
                    }
                }
            }

            folder.Children = children;
            return folder;
        }

        private async Task<List<string>> ExploreFileAsync(string directoryPath)
        {
            ShareDirectoryClient directoryClient = _shareClientUserShare.GetDirectoryClient(directoryPath);

            if (!await directoryClient.ExistsAsync())
            {
                throw new Exception(string.Format(StardardMessages.DirectoryDoesNotExist, directoryPath));
            }

            var children = new List<string>();

            var directories = directoryClient.GetFilesAndDirectoriesAsync();
            if (directories == null)
            {
                throw new Exception(string.Format(StardardMessages.NoFilesFoundInDirectory, directoryPath));
            }

            await foreach (ShareFileItem item in directories)
            {
                if (item is ShareFileItem fileItem)
                {
                    if (fileItem.IsDirectory)
                    {
                        children.AddRange(await ExploreFileAsync($"{directoryPath}/{item.Name}"));
                    }
                    else
                    {
                        children.Add($"{directoryPath}/{item.Name}");
                    }
                }
            }

            return children;
        }

        private async Task<List<FileInfo>> ExploreFileInfo(string directoryPath)
        {
            try
            {
                ShareDirectoryClient directoryClient = _shareClientUserShare.GetDirectoryClient(directoryPath);

                if (!await directoryClient.ExistsAsync())
                {
                    throw new Exception(string.Format(StardardMessages.DirectoryDoesNotExist, directoryPath));
                }

                var children = new List<FileInfo>();

                var directories = directoryClient.GetFilesAndDirectoriesAsync();
                if (directories == null)
                {
                    throw new Exception(string.Format(StardardMessages.NoFilesFoundInDirectory, directoryPath));
                }
                string fileName;
                string content;
                ShareFileClient shareFileClient;
                int dataCount = 0;

                await foreach (ShareFileItem item in directories)
                {
                    if (item is ShareFileItem fileItem)
                    {
                        if (fileItem.IsDirectory)
                        {
                            children.AddRange(await ExploreFileInfo($"{directoryPath}/{item.Name}"));
                        }
                        else
                        {
                            content = "";
                            dataCount = 0;
                            fileName = Path.GetFileName(fileItem.Name);

                            shareFileClient = directoryClient.GetFileClient(fileName);

                            if (!await shareFileClient.ExistsAsync())
                            {
                                throw new Exception(string.Format(StardardMessages.FileDoesNotExist, fileItem.Name));
                            }

                            using (Stream download = await shareFileClient.OpenReadAsync())
                            {
                                using (StreamReader reader = new StreamReader(download))
                                {
                                    content = await reader.ReadToEndAsync();
                                }
                            }

                            if (!string.IsNullOrWhiteSpace(content))
                            {
                                dataCount = content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).Length - 1;
                            }

                            children.Add(new FileInfo
                            {
                                FileName = item.Name,
                                FilePath = $"{directoryPath}/{item.Name}",
                                FileUri = directoryClient.GetFileClient(item.Name).Uri.ToString(),
                                FileContent = content,
                                RecordCount = dataCount
                            });
                        }
                    }
                }

                return children;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string GenerateAcceptedFileContent(string fileContent, List<int> errorsRowNums)
        {
            var fileContentRows = fileContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            if (errorsRowNums.Any())
            {
                fileContentRows = fileContentRows.Where((row, index) => !errorsRowNums.Contains(index + 1)).ToList();

                //If all rows status are errors, only header row left, return empty.
                if (fileContentRows.Count == 1)
                {
                    return "";
                }
                else
                {
                    return string.Join(Environment.NewLine, fileContentRows);
                }
            }
            else
            {
                //No errors, return all rows.
                return fileContent;
            }
        }

        private string GenerateRejectedFileContent(string fileContent, List<int> errorAndCriticalRowNums)
        {
            var fileContentRows = fileContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            var header = fileContentRows.FirstOrDefault();
            if (errorAndCriticalRowNums.Any())
            {
                header = "Line Number|" + header;
                fileContentRows = fileContentRows.Where((row, index) => errorAndCriticalRowNums.Contains(index + 1)).ToList();

                for (int i = 0; i < fileContentRows.Count; i++)
                {
                    fileContentRows[i] = errorAndCriticalRowNums[i] + "|" + fileContentRows[i];
                }

                fileContentRows.Insert(0, header);
            }

            return string.Join(Environment.NewLine, fileContentRows);
        }

        /// <summary>
        /// Bug 20306783: [QA Bug] - Validation Timestamp format in SUMMARY .txt file
        /// </summary>
        /// <param name="acceptedRecords"></param>
        /// <param name="rejectedRecords"></param>
        /// <param name="userEmail"></param>
        /// <param name="validationTimeStamp"></param>
        /// <returns></returns>
        private string GenerateSummaryFileContent(int acceptedRecords, int rejectedRecords, string userEmail, DateTime validationTimeStamp)
        {
            return $"Accepted Records: {acceptedRecords}{Environment.NewLine}" +
                   $"Rejected Records: {rejectedRecords}{Environment.NewLine}" +
                   $"Validated by: {userEmail}{Environment.NewLine}" +
                   $"Validation Timestamp: {validationTimeStamp.ToString("yyyyMMdd HH:mm")}";
        }

        private DateTime? GetFileDateInfo(Response<ShareFileProperties> properties, bool isCreationDate = true)
        {
            //This FileCreatedOn is the true modify date.
            //This FileChangedOn is the true modify date.
            if (isCreationDate)
            {
                if (!properties.Value.SmbProperties.FileCreatedOn.HasValue)
                {
                    return null;
                }
                else
                {
                    return properties.Value.SmbProperties.FileCreatedOn.Value.ToUniversalTime().DateTime;
                }
            }
            else
            {
                if (!properties.Value.SmbProperties.FileLastWrittenOn.HasValue)
                {
                    return null;
                }
                else
                {
                    return properties.Value.SmbProperties.FileLastWrittenOn.Value.ToUniversalTime().DateTime;
                }
            }
        }

    }
}
