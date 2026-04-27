using DVT.Core.FileLoader;
using DVT.Core.Models;
using System;
using static DVT.Core.Constants;

namespace DVT.Core.Services
{
    public class FileLoadService : IFileLoadService
    {
        private readonly IStorageService _storageService;
        private readonly IActivityLogService _activityLogService;

        public FileLoadService(IStorageService storageService, IActivityLogService activityLogService)
        {
            _storageService = storageService;
            _activityLogService = activityLogService;
        }

        public async Task<JobLoadResult> LoadFile(JobLoad jobLoad)
        {
            var jobLoadResult = new JobLoadResult
            {
                Operation = Operations.UploadFiles
            };

            var userEmail = jobLoad.UserEmail;
            var message = "";
            foreach (var fileLoad in jobLoad.FileList)
            {
                var fileLoadResult = new FileLoadResult();
                var jobFileId = fileLoad.JobFileId;
                var jobFileName = fileLoad.FileName;
                fileLoadResult.JobFileId = jobFileId;

                try
                {
                    switch (fileLoad.FileType)
                    {
                        case "Vir":
                            {
                                IFileLoader _fileLoader = new VirFileLoader();
                                fileLoadResult = await _fileLoader.LoadFileAsync(fileLoad, _storageService);
                            }
                            break;
                        case "Supplier":
                            {
                                IFileLoader _fileLoader = new SupplierFileLoader();
                                fileLoadResult = await _fileLoader.LoadFileAsync(fileLoad, _storageService);
                            }
                            break;
                        case "Item":
                            {
                                IFileLoader _fileLoader = new ItemFileLoader();
                                fileLoadResult = await _fileLoader.LoadFileAsync(fileLoad, _storageService);
                            }
                            break;
                        case "Inventory":
                            {
                                IFileLoader _fileLoader = new InventoryFileLoader();
                                fileLoadResult = await _fileLoader.LoadFileAsync(fileLoad, _storageService);
                            }
                            break;
                        case "Po":
                            {
                                IFileLoader _fileLoader = new POFileLoader();
                                fileLoadResult = await _fileLoader.LoadFileAsync(fileLoad, _storageService);
                            }
                            break;
                        case "PoItem":
                            {
                                IFileLoader _fileLoader = new POItemFileLoader();
                                fileLoadResult = await _fileLoader.LoadFileAsync(fileLoad, _storageService);
                            }
                            break;
                        case "Uom":
                            {
                                IFileLoader _fileLoader = new UOMFileLoader();
                                fileLoadResult = await _fileLoader.LoadFileAsync(fileLoad, _storageService);
                            }
                            break;
                        case "Mpn":
                            {
                                IFileLoader _fileLoader = new MPNFileLoader();
                                fileLoadResult = await _fileLoader.LoadFileAsync(fileLoad, _storageService);
                            }
                            break;
                        default:
                            fileLoadResult.Message = "Unknown file template: " + fileLoad.FileType;
                            break;
                    }

                    message = fileLoadResult.Message;
                }
                catch (Exception ex)
                {
                    fileLoadResult.Success = false;
                    fileLoadResult.JobFileId = jobFileId;
                    fileLoadResult.Message = ex.Message;
                    message = string.Format(StardardMessages.LoadFileError, jobFileName, ex.Message);
                }

                _activityLogService.AddLogAsync(new ActivityLog
                {
                    LogId = Guid.NewGuid(),
                    EntityId = jobFileId,
                    Entity = DVTEntities.JobFile,
                    Message = message,
                    CreateBy = userEmail,
                    CreateDate = DateTime.UtcNow
                }).Wait();

                jobLoadResult.FileLoadResults.Add(fileLoadResult);
            }

            return jobLoadResult;
        }
    }
}
