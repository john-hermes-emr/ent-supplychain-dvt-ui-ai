using DVT.Core.Helper;
using DVT.Core.Models;
using Newtonsoft.Json;
using System.Dynamic;
using static DVT.Core.Constants;

namespace DVT.Core.Services
{
    public class JobFileService : IJobFileService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IActivityLogService _activityLogService;
        private readonly IStorageService _storageService;

        public JobFileService(IUnitOfWork unitOfWork, IActivityLogService activityLogService, IStorageService storageService)
        {
            _unitOfWork = unitOfWork;
            _activityLogService = activityLogService;
            _storageService = storageService;
        }

        public async ValueTask<JobFile> GetJobFileByIdAsync(Guid jobFileId)
        {
            var jobFile = await _unitOfWork.JobFiles.GetByIdAsync(jobFileId);
            if (jobFile == null)
            {
                throw new KeyNotFoundException($"{StardardMessages.ItemNotFound} for Job File Id: {jobFileId}");
            }
            return jobFile;
        }
        public async ValueTask<JobFile> GetJobFileByIdNoValidationAsync(Guid jobFileId)
        {
            var jobFile = await _unitOfWork.JobFiles.GetByIdNoValidationAsync(jobFileId);
            if (jobFile == null)
            {
                throw new KeyNotFoundException($"{StardardMessages.ItemNotFound} for Job File Id: {jobFileId}");
            }
            return jobFile;
        }

        public async ValueTask<IEnumerable<JobFile>> GetJobFilesByJobIdAsync(Guid jobId)
        {
            var jobFiles = await _unitOfWork.JobFiles.GetJobFilesByJobIdAsync(jobId);
            if (jobFiles == null)
            {
                throw new KeyNotFoundException($"{StardardMessages.NoJobFilesFound} for Job Id: {jobId}");
            }
            return jobFiles;
        }

        public async ValueTask<IEnumerable<JobFile>> GetJobFilesByJobIdNoValidationAsync(Guid jobId)
        {
            var jobFiles = await _unitOfWork.JobFiles.GetJobFilesByJobIdNoValidationAsync(jobId);
            if (jobFiles == null)
            {
                throw new KeyNotFoundException($"{StardardMessages.NoJobFilesFound} for Job Id: {jobId}");
            }
            return jobFiles;
        }

        public async ValueTask<IEnumerable<JobFile>> GetJobFilesByJobIdNoValidationNoTrackingAsync(Guid jobId)
        {
            var jobFiles = await _unitOfWork.JobFiles.GetJobFilesByJobIdNoValidationNoTrackingAsync(jobId);
            if (jobFiles == null)
            {
                throw new KeyNotFoundException($"{StardardMessages.NoJobFilesFound} for Job Id: {jobId}");
            }
            return jobFiles;
        }

        public async ValueTask<OperationResult> UpdateJobFileValidationResultAsync(Guid jobFileId, string validationMessages, string validationStats, string status, string updateBy)
        {
            var jobFile = await _unitOfWork.JobFiles.GetByIdAsync(jobFileId);
            if (jobFile == null)
            {
                throw new KeyNotFoundException($"{StardardMessages.ItemNotFound} id: {jobFileId}");
            }

            //Write the validation results to a file in the working directory. For performance reasons, we don't want to write large JSON to the database table.
            await _storageService.CreateTextFileInWorkingFolderAsync(jobFile.JobId, jobFile.GetValidationMessageFileName(), validationMessages);            

            jobFile.ValidationStats = validationStats;
            jobFile.Status = status;            
            jobFile.UpdateBy = updateBy;
            jobFile.UpdateDate = DateTime.UtcNow;
            jobFile.LoadDate = DateTime.UtcNow;
            await _unitOfWork.CommitAsync();

            await _activityLogService.AddLogAsync(new ActivityLog
            {
                LogId = Guid.NewGuid(),
                EntityId = jobFile.JobFileId,
                Entity = DVTEntities.JobFile,
                Message = string.Format(StardardMessages.FileVerified, jobFile.FileName, status),
                CreateBy = updateBy,
                CreateDate = DateTime.UtcNow
            });

            //Don't need to return the job file since we're not using it.
            return new OperationResult
            {
                Operation = Operations.SetJobFileValidationResult,
                Success = true,
                Message = StardardMessages.SetJobFileValidationResultSuccessfully
            };
        }

        public async ValueTask<OperationResult> UpdateJobFileStatusAsync(Guid jobFileId, string status, string updateBy)
        {
            var jobFile = await _unitOfWork.JobFiles.GetByIdAsync(jobFileId);
            if (jobFile == null)
            {
                throw new KeyNotFoundException($"{StardardMessages.ItemNotFound} id: {jobFileId}");
            }

            var constantFileStatus = GetJobFileStatus(status);

            if (string.IsNullOrEmpty(constantFileStatus))
            {
                throw new ArgumentNullException(nameof(status), StardardMessages.IncorrectJobFileStatus);
            }

            jobFile.Status = constantFileStatus;
            jobFile.UpdateBy = updateBy;
            jobFile.UpdateDate = DateTime.UtcNow;
            jobFile.LoadDate = DateTime.UtcNow;
            await _unitOfWork.CommitAsync();

            await _activityLogService.AddLogAsync(new ActivityLog
            {
                LogId = Guid.NewGuid(),
                EntityId = jobFileId,
                Entity = DVTEntities.JobFile,
                Message = Operations.UpdateJobFileStatus + " " + string.Format(ValidationMessages.ChangedFileStatus, jobFile.FileName, constantFileStatus),
                CreateBy = updateBy,
                CreateDate = DateTime.UtcNow
            });

            return new OperationResult
            {
                Operation = Operations.UpdateJobFileStatus,
                Success = true,
                Message = StardardMessages.JobFileStatusUpdatedSuccessfully,
                Data = jobFile
            };
        }

        public async ValueTask<OperationResult> BatchUpdateJobFilesStatusAsync(List<Guid> jobFileIds, string status, string updateBy)
        {
            var constantFileStatus = GetJobFileStatus(status);

            if (string.IsNullOrEmpty(constantFileStatus))
            {
                throw new ArgumentNullException(nameof(status), StardardMessages.IncorrectJobFileStatus);
            }

            var jobFiles = await _unitOfWork.JobFiles.GetJobFilesByJobFileIdsNoValidationAsync(jobFileIds);

            if (jobFiles == null || !jobFiles.Any())
            {
                throw new KeyNotFoundException($"{StardardMessages.ItemNotFound} for Job File Ids: {string.Join(", ", jobFileIds)}");
            }

            foreach (var jobFile in jobFiles)
            {
                jobFile.Status = constantFileStatus;
                jobFile.UpdateBy = updateBy;
                jobFile.UpdateDate = DateTime.UtcNow;
                jobFile.LoadDate = DateTime.UtcNow;
            }

            await _unitOfWork.CommitAsync();

            await _activityLogService.AddLogAsync(new ActivityLog
            {
                LogId = Guid.NewGuid(),
                EntityId = jobFileIds.First(),
                Entity = DVTEntities.JobFile,
                Message = Operations.UpdateJobFileStatus + " " + string.Format(ValidationMessages.ChangedFileStatus, string.Join(", ", jobFileIds), constantFileStatus),
                CreateBy = updateBy,
                CreateDate = DateTime.UtcNow
            });

            return new OperationResult
            {
                Operation = Operations.UpdateJobFileStatus,
                Success = true,
                Message = StardardMessages.JobFileStatusUpdatedSuccessfully,
                Data = jobFiles
            };
        }

        private string GetJobFileStatus(string status)
        {
            if (status.Equals(WellKnownFileStatuses.New, StringComparison.OrdinalIgnoreCase))
            {
                return WellKnownFileStatuses.New;
            }
            else if (status.Equals(WellKnownFileStatuses.Uploaded, StringComparison.OrdinalIgnoreCase))
            {
                return WellKnownFileStatuses.Uploaded;
            }
            else if (status.Equals(WellKnownFileStatuses.InProgress, StringComparison.OrdinalIgnoreCase))
            {
                return WellKnownFileStatuses.InProgress;
            }
            else if (status.Equals(WellKnownFileStatuses.Warning, StringComparison.OrdinalIgnoreCase))
            {
                return WellKnownFileStatuses.Warning;
            }
            else if (status.Equals(WellKnownFileStatuses.Errors, StringComparison.OrdinalIgnoreCase))
            {
                return WellKnownFileStatuses.Errors;
            }
            else if (status.Equals(WellKnownFileStatuses.Critical, StringComparison.OrdinalIgnoreCase))
            {
                return WellKnownFileStatuses.Critical;
            }
            else if (status.Equals(WellKnownFileStatuses.Validated, StringComparison.OrdinalIgnoreCase))
            {
                return WellKnownFileStatuses.Validated;
            }
            else if (status.Equals(WellKnownFileStatuses.Failed, StringComparison.OrdinalIgnoreCase))
            {
                return WellKnownFileStatuses.Failed;
            }
            else if (status.Equals(WellKnownFileStatuses.Accepted, StringComparison.OrdinalIgnoreCase))
            {
                return WellKnownFileStatuses.Accepted;
            }
            else
            {
                return "";
            }
        }

        public async ValueTask<OperationResult> UpdateJobFilesStatusByJobIdAsync(Guid jobId, string status, string updateBy)
        {
            var jobFiles = await _unitOfWork.JobFiles.GetJobFilesByJobIdNoValidationAsync(jobId);
            if (jobFiles == null || !jobFiles.Any())
            {
                throw new KeyNotFoundException($"{StardardMessages.ItemNotFound} for Job Id: {jobId}");
            }

            var activityLogs = new List<ActivityLog>();
            foreach (var jobFile in jobFiles)
            {
                jobFile.Status = status;
                jobFile.UpdateBy = updateBy;
                jobFile.UpdateDate = DateTime.UtcNow;
                jobFile.LoadDate = DateTime.UtcNow;
                activityLogs.Add(new ActivityLog
                {
                    LogId = Guid.NewGuid(),
                    EntityId = jobFile.JobFileId,
                    Entity = DVTEntities.JobFile,
                    Message = Operations.UpdateJobFileStatus,
                    CreateBy = updateBy,
                    CreateDate = DateTime.UtcNow
                });
            }

            await _unitOfWork.CommitAsync();

            await _activityLogService.AddLogsAsync(activityLogs);

            return new OperationResult
            {
                Operation = Operations.UpdateJobFileStatus,
                Success = true,
                Message = StardardMessages.JobFileStatusUpdatedSuccessfully,
                Data = jobFiles
            };
        }

        public async ValueTask<OperationResult> DeleteJobFilesAsync(Guid jobId, string updateBy, bool isRefresh = false)
        {
            var files = await GetJobFilesByJobIdNoValidationAsync(jobId);

            var activityLogs = new List<ActivityLog>();

            foreach (var file in files)
            {
                file.Deleted = true;
                file.UpdateDate = DateTime.UtcNow;
                file.UpdateBy = updateBy;
                file.LoadDate = DateTime.UtcNow;
                activityLogs.Add(new ActivityLog
                {
                    LogId = Guid.NewGuid(),
                    EntityId = file.JobFileId,
                    Entity = DVTEntities.JobFile,
                    Message = isRefresh ? Operations.RefreshAndDeleteJobFile : Operations.DeleteJobFile,
                    CreateBy = updateBy,
                    CreateDate = DateTime.UtcNow
                });
            }

            await _unitOfWork.CommitAsync();

            await _activityLogService.AddLogsAsync(activityLogs);

            //delete files from storage
            await _storageService.DeleteJobFilesAsync(jobId);

            return new OperationResult
            {
                Operation = Operations.DeleteJobFile,
                Success = true,
                Message = StardardMessages.JobFilesDeletedSuccessfully
            };
        }

        /// <summary>
        /// User Story 16164142: DVT - Analysis Controller - Get Analysis Errors per file
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="jobFileId"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public async ValueTask<OperationResult> GetJobFileValidationMessageByJobIdAndJobFileIdAsync(Guid jobId, Guid jobFileId, string userEmail)
        {
            var jobFiles = await _unitOfWork.JobFiles.GetJobFilesByJobIdAsync(jobId);
            if (jobFiles == null)
            {
                throw new KeyNotFoundException($"{StardardMessages.ItemNotFound} for Job Id: {jobId}");
            }

            var jobFile = jobFiles.FirstOrDefault(x => x.JobFileId == jobFileId);
            if (jobFile == null)
            {
                throw new KeyNotFoundException($"{StardardMessages.ItemNotFound} for Job File Id: {jobFileId}");
            }

            var result = new OperationResult()
            {
                Operation = Operations.GetJobFileValidationErrors,
                Success = true,
            };

            var errorResult = new FileValidationErrorResult
            {
                FileName = jobFile.FileName,
                Date = jobFile.LoadDate.Value,
                FileType = jobFile.FileType,
                TableName = jobFile.TableName,
            };           

            try
            {
                //Get the Validation Results from the file saved in the working directory
                string validationFileContents = await _storageService.GetWorkingFileContentsAsync(jobFile.JobId, jobFile.GetValidationMessageFileName());

                if (string.IsNullOrWhiteSpace(validationFileContents))
                {
                    result.Message = StardardMessages.NoErrorFound;
                    result.Data = null;
                    return result;
                }

                var validationResult = JsonConvert.DeserializeObject<List<FileRowValidationResult>>(validationFileContents);

                if (validationResult == null || !validationResult.Any())
                {
                    result.Message = StardardMessages.NoErrorFound;
                    result.Data = errorResult;
                    return result;
                }

                var allErrors = new List<FileValidationSummarized>();
                var allErrorsCopy = new List<FileValidationSummarized>();

                var errors = validationResult.Where(x => x.ValidationResult != null && x.ValidationResult.Errors.Any());

                if (errors == null || !errors.Any())
                {
                    result.Message = StardardMessages.NoErrorFound;
                    result.Data = errorResult;
                    return result;
                }

                var errorCount = 0;
                foreach (var error in errors)
                {
                    errorCount += error.ValidationResult.Errors.Count;
                    error.ValidationResult.Errors.ForEach(x =>
                    {
                        allErrors.Add(new FileValidationSummarized
                        {
                            MessageType = x.ErrorCode,
                            Field = x.PropertyName,
                            Error = x.ErrorMessage.ToUpper()
                        });

                        allErrorsCopy.Add(new FileValidationSummarized
                        {
                            MessageType = x.ErrorCode,
                            Field = x.PropertyName,
                            Error = x.ErrorMessage
                        });
                    });
                }

                var groupData = allErrors.GroupBy(x => new { x.MessageType, x.Field, x.Error });

                errorResult.Summarizeds = groupData.Select(g => new FileValidationSummarized
                {
                    GroupId = Guid.NewGuid(),
                    MessageType = g.Key.MessageType,
                    Field = g.Key.Field,
                    Error = allErrorsCopy.First(x => x.Error.Equals(g.Key.Error, StringComparison.OrdinalIgnoreCase)).Error,
                    Count = g.Count(),
                }).ToList();

                foreach (var error in errorResult.Summarizeds)
                {
                    var data = validationResult.Where(vr => vr.ValidationResult != null && vr.ValidationResult.Errors.Any(e => e.PropertyName == error.Field && e.ErrorMessage.Equals(error.Error, StringComparison.OrdinalIgnoreCase) && string.Equals(e.ErrorCode, error.MessageType, StringComparison.OrdinalIgnoreCase)));

                    data = data.OrderBy(d => d.RowNumber).ToList();

                    foreach (var item in data)
                    {
                        item.ValidationResult.Errors.Where(e => e.PropertyName == error.Field && e.ErrorMessage.Equals(error.Error, StringComparison.OrdinalIgnoreCase)).ToList()
                            .ForEach(e => error.Details.Add(new FileRowValidationSummarizedDetail
                            {
                                GroupId = error.GroupId,
                                RowNumber = item.RowNumber == -1 ? "N/A" : item.RowNumber.ToString(),
                                Problem = error.MessageType + " " + e.PropertyName,
                                ErrorDescription = e.ErrorMessage,
                                Data = e.AttemptedValue?.ToString(),
                            }));
                    }
                }

                result.Data = errorResult;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Exception = new Exception(ex.Message);
                result.Message = StardardMessages.ErrorOccurredWhileProcessingValidationErrors;
                await _activityLogService.AddLogAsync(new ActivityLog
                {
                    LogId = Guid.NewGuid(),
                    EntityId = jobId,
                    Entity = DVTEntities.Job,
                    MessageType = LogMessageTypes.Error,
                    Message = Operations.GetJobFileValidationErrors + ", Exception Message: " + result.Message + ", " + ex.Message,
                    CreateBy = userEmail,
                    CreateDate = DateTime.UtcNow
                });
            }

            return result;
        }

        public async ValueTask<string> GetJobFileValidationFileContentsByJobFileAsync(JobFile jobFile)
        {
            try
            {
                return await _storageService.GetWorkingFileContentsAsync(jobFile.JobId, jobFile.GetValidationMessageFileName());
            }
            catch(Exception ex)
            {
                throw new Exception($"Error getting JobFile validation contents by JobFile. JobId:{jobFile.JobId}, JobFileId:{jobFile.JobFileId}", ex);
            }
        }

        /// <summary>
        /// Task 19302250: DVT - Analysis Controller - Generate Analysis Errors report service
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="jobFileId"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public async ValueTask<byte[]> GenerateJobFileErrorReportByJobIdAndJobFileIdAsync(Guid jobId, Guid jobFileId, string userEmail)
        {
            var result = await GetJobFileValidationMessageByJobIdAndJobFileIdAsync(jobId, jobFileId, userEmail);

            var headers = ErrorReportHeaderList;

            var exportList = new List<dynamic>();

            if (!result.Success || result.Data == null)
            {
                return ExcelHelper.ExportToExcel(null, headers, null);
            }

            var errors = result.Data as FileValidationErrorResult;

            var fileInfos = new List<string>();

            if (errors != null)
            {
                fileInfos.Add(errors.Date.ToString("MM/dd/yyyy"));
                fileInfos.Add(errors.TableName);
                fileInfos.Add($"File Name: {errors.FileName}");

                var status = "";
                foreach (var summarized in errors.Summarizeds)
                {
                    status = summarized.MessageType;
                    foreach (var detail in summarized.Details)
                    {
                        dynamic expandoObject = new ExpandoObject();

                        AddProperty(expandoObject, ErrorReportHeaders.RowNumber, detail.RowNumber == "-1" ? "N/A" : detail.RowNumber);

                        AddProperty(expandoObject, ErrorReportHeaders.Problem, detail.Problem);

                        AddProperty(expandoObject, ErrorReportHeaders.ValidationMessage, detail.ErrorDescription);

                        AddProperty(expandoObject, ErrorReportHeaders.Data, detail.Data);

                        //AddProperty(expandoObject, ErrorReportHeaders.Reference, "Reference");

                        exportList.Add(expandoObject);
                    }
                }
            }

            return ExcelHelper.ExportToExcel(fileInfos, headers, exportList);
        }

        private void AddProperty(ExpandoObject expandoObject, string name, object value)
        {
            var obj = (IDictionary<string, object>)expandoObject;
            if (!obj.ContainsKey(name))
            {
                obj.Add(name, value);
            }
        }

        public async ValueTask<OperationResult> GetJobValidationStatsByJobIdAndJobFileIdAsync(Guid jobId, Guid jobFileId, string userEmail)
        {
            {
                var jobFiles = await _unitOfWork.JobFiles.GetJobFilesByJobIdAsync(jobId);
                if (jobFiles == null)
                {
                    throw new KeyNotFoundException($"{StardardMessages.ItemNotFound} for Job Id: {jobId}");
                }

                var jobFile = jobFiles.FirstOrDefault(jf => jf.JobFileId == jobFileId);

                if (jobFile == null)
                {
                    throw new KeyNotFoundException($"{StardardMessages.ItemNotFound} for Job File Id: {jobFileId}");
                }

                var result = new OperationResult()
                {
                    Operation = Operations.GetJobFileStatistics,
                    Success = true,
                };

                try
                {

                    var fileType = jobFile.FileType;
                    var fileName = jobFile.FileName;

                    var fileValidationStatsResult = new FileValidationStatsResult
                    {
                        FileName = fileName,
                        Date = jobFile.LoadDate.Value,
                        FileType = jobFile.FileType,
                        TableName = jobFile.TableName,
                    };

                    var validationStats = jobFile.ValidationStats;

                    if (string.Equals(fileType, FileTypes.Vir, StringComparison.OrdinalIgnoreCase))
                    {
                        fileValidationStatsResult.Stats = string.IsNullOrWhiteSpace(validationStats) ? null : JsonConvert.DeserializeObject<FileCalculateStatistics_Vir>(validationStats);
                    }
                    else if (string.Equals(fileType, FileTypes.Item, StringComparison.OrdinalIgnoreCase))
                    {
                        fileValidationStatsResult.Stats = string.IsNullOrWhiteSpace(validationStats) ? null : JsonConvert.DeserializeObject<FileCalculateStatistics_Item>(validationStats);
                    }
                    else if (string.Equals(fileType, FileTypes.Supplier, StringComparison.OrdinalIgnoreCase))
                    {
                        fileValidationStatsResult.Stats = string.IsNullOrWhiteSpace(validationStats) ? null : JsonConvert.DeserializeObject<FileCalculateStatistics_Supplier>(validationStats);
                    }
                    else if (string.Equals(fileType, FileTypes.Inventory, StringComparison.OrdinalIgnoreCase))
                    {
                        fileValidationStatsResult.Stats = string.IsNullOrWhiteSpace(validationStats) ? null : JsonConvert.DeserializeObject<FileCalculateStatistics_Inventory>(validationStats);
                    }
                    else if (string.Equals(fileType, FileTypes.Po, StringComparison.OrdinalIgnoreCase))
                    {
                        fileValidationStatsResult.Stats = string.IsNullOrWhiteSpace(validationStats) ? null : JsonConvert.DeserializeObject<FileCalculateStatistics_PO>(validationStats);
                    }
                    else if (string.Equals(fileType, FileTypes.PoItem, StringComparison.OrdinalIgnoreCase))
                    {
                        fileValidationStatsResult.Stats = string.IsNullOrWhiteSpace(validationStats) ? null : JsonConvert.DeserializeObject<FileCalculateStatistics_POItem>(validationStats);
                    }
                    else if (string.Equals(fileType, FileTypes.Uom, StringComparison.OrdinalIgnoreCase))
                    {
                        fileValidationStatsResult.Stats = string.IsNullOrWhiteSpace(validationStats) ? null : JsonConvert.DeserializeObject<FileCalculateStatistics_UOM>(validationStats);
                    }
                    else if (string.Equals(fileType, FileTypes.Mpn, StringComparison.OrdinalIgnoreCase))
                    {
                        fileValidationStatsResult.Stats = string.IsNullOrWhiteSpace(validationStats) ? null : JsonConvert.DeserializeObject<FileCalculateStatistics_MPN>(validationStats);
                    }

                    result.Data = fileValidationStatsResult;
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Exception = new Exception(ex.Message);
                    result.Message = StardardMessages.ErrorOccurredWhileProcessingValidationStatistics;
                    await _activityLogService.AddLogAsync(new ActivityLog
                    {
                        LogId = Guid.NewGuid(),
                        EntityId = jobId,
                        Entity = DVTEntities.Job,
                        MessageType = LogMessageTypes.Error,
                        Message = Operations.GetJobFileStatistics + ", Exception Message: " + result.Message + ", " + ex.Message,
                        CreateBy = userEmail,
                        CreateDate = DateTime.UtcNow
                    });
                }
                return result;
            }
        }

        /// <summary>
        /// User Story 16255413: Validation Service - Generate statistics Report for Vir - API Controller
        /// Task 19298968: Validation Service - Generate statistics Report for Vir - API Service
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public async ValueTask<byte[]> GenerateJobFileStatsReportByJobIdAndJobFileIdAsync(Guid jobId, Guid jobFileId, string userEmail)
        {
            var result = await GetJobValidationStatsByJobIdAndJobFileIdAsync(jobId, jobFileId, userEmail);

            var headers = StatisticsReportHeadersList;

            var exportList = new List<dynamic>();

            if (!result.Success || result.Data == null)
            {
                return ExcelHelper.ExportToExcel(null, headers, null);
            }

            var stats = result.Data as FileValidationStatsResult;

            var fileInfos = new List<string>();

            if (stats != null)
            {
                fileInfos.Add(stats.Date.ToString("MM/dd/yyyy"));
                fileInfos.Add(stats.TableName);
                fileInfos.Add($"File Name: {stats.FileName}");

                exportList = GenerateStatsData(stats);
            }

            return ExcelHelper.ExportToExcel(fileInfos, headers, exportList);
        }

        private List<dynamic> GenerateStatsData(FileValidationStatsResult stats)
        {
            var exportList = new List<dynamic>();

            if (string.Equals(stats.FileType, FileTypes.Vir, StringComparison.OrdinalIgnoreCase))
            {
                GenerateVirStatsData(stats, exportList);
            }
            else if (string.Equals(stats.FileType, FileTypes.Item, StringComparison.OrdinalIgnoreCase))
            {
                GenerateItemStatsData(stats, exportList);
            }
            else if (string.Equals(stats.FileType, FileTypes.Supplier, StringComparison.OrdinalIgnoreCase))
            {
                GenerateSupplierStatsData(stats, exportList);
            }
            else if (string.Equals(stats.FileType, FileTypes.Inventory, StringComparison.OrdinalIgnoreCase))
            {
                GenerateInventoryStatsData(stats, exportList);
            }
            else if (string.Equals(stats.FileType, FileTypes.Po, StringComparison.OrdinalIgnoreCase))
            {
                GeneratePOStatsData(stats, exportList);
            }
            else if (string.Equals(stats.FileType, FileTypes.PoItem, StringComparison.OrdinalIgnoreCase))
            {
                GeneratePOItemStatsData(stats, exportList);
            }
            else if (string.Equals(stats.FileType, FileTypes.Uom, StringComparison.OrdinalIgnoreCase))
            {
                GenerateUOMStatsData(stats, exportList);
            }
            else if (string.Equals(stats.FileType, FileTypes.Mpn, StringComparison.OrdinalIgnoreCase))
            {
                GenerateMPNStatsData(stats, exportList);
            }

            return exportList;
        }

        private void GenerateVirStatsData(FileValidationStatsResult stats, List<dynamic> exportList)
        {
            dynamic expandoObject = new ExpandoObject();

            var virStats = stats.Stats as FileCalculateStatistics_Vir;
            AddProperty(expandoObject, StatisticsReportHeaders.FieldName, VirStatisticsReportFieldNames.TotalRecords);
            AddProperty(expandoObject, StatisticsReportHeaders.Min, "");
            AddProperty(expandoObject, StatisticsReportHeaders.Max, virStats?.TotalRecords);
            exportList.Add(expandoObject);

            expandoObject = new ExpandoObject();
            AddProperty(expandoObject, StatisticsReportHeaders.FieldName, VirStatisticsReportFieldNames.QuantityOrdered);
            AddProperty(expandoObject, StatisticsReportHeaders.Min, virStats?.QuantityOrderedMin);
            AddProperty(expandoObject, StatisticsReportHeaders.Max, virStats?.QuantityOrderedMax);
            exportList.Add(expandoObject);

            expandoObject = new ExpandoObject();
            AddProperty(expandoObject, StatisticsReportHeaders.FieldName, VirStatisticsReportFieldNames.QuantityReceived);
            AddProperty(expandoObject, StatisticsReportHeaders.Min, virStats?.QuantityReceivedMin);
            AddProperty(expandoObject, StatisticsReportHeaders.Max, virStats?.QuantityReceivedMax);
            exportList.Add(expandoObject);

            expandoObject = new ExpandoObject();
            AddProperty(expandoObject, StatisticsReportHeaders.FieldName, VirStatisticsReportFieldNames.DateReceived);
            AddProperty(expandoObject, StatisticsReportHeaders.Min, virStats?.DateReceivedMin);
            AddProperty(expandoObject, StatisticsReportHeaders.Max, virStats?.DateReceivedMax);
            exportList.Add(expandoObject);

            expandoObject = new ExpandoObject();
            AddProperty(expandoObject, StatisticsReportHeaders.FieldName, VirStatisticsReportFieldNames.InvoicePricePaid);
            AddProperty(expandoObject, StatisticsReportHeaders.Min, virStats?.InvoicePricePaidMin);
            AddProperty(expandoObject, StatisticsReportHeaders.Max, virStats?.InvoicePricePaidMax);
            exportList.Add(expandoObject);

            expandoObject = new ExpandoObject();
            AddProperty(expandoObject, StatisticsReportHeaders.FieldName, VirStatisticsReportFieldNames.UnitPrice);
            AddProperty(expandoObject, StatisticsReportHeaders.Min, virStats?.UnitPriceMin);
            AddProperty(expandoObject, StatisticsReportHeaders.Max, virStats?.UnitPriceMax);
            exportList.Add(expandoObject);

            expandoObject = new ExpandoObject();
            AddProperty(expandoObject, StatisticsReportHeaders.FieldName, VirStatisticsReportFieldNames.CommittedDate);
            AddProperty(expandoObject, StatisticsReportHeaders.Min, virStats?.CommittedDateMin);
            AddProperty(expandoObject, StatisticsReportHeaders.Max, virStats?.CommittedDateMax);
            exportList.Add(expandoObject);
        }

        private void GenerateItemStatsData(FileValidationStatsResult stats, List<dynamic> exportList)
        {
            dynamic expandoObject = new ExpandoObject();

            var itemStats = stats.Stats as FileCalculateStatistics_Item;
            AddProperty(expandoObject, StatisticsReportHeaders.FieldName, ItemStatisticsReportFieldNames.TotalRecords);
            AddProperty(expandoObject, StatisticsReportHeaders.Min, "");
            AddProperty(expandoObject, StatisticsReportHeaders.Max, itemStats?.TotalRecords);
            exportList.Add(expandoObject);

            expandoObject = new ExpandoObject();
            AddProperty(expandoObject, StatisticsReportHeaders.FieldName, ItemStatisticsReportFieldNames.StandardCost);
            AddProperty(expandoObject, StatisticsReportHeaders.Min, itemStats?.StandardCostMin);
            AddProperty(expandoObject, StatisticsReportHeaders.Max, itemStats?.StandardCostMax);
            exportList.Add(expandoObject);
        }

        private void GenerateSupplierStatsData(FileValidationStatsResult stats, List<dynamic> exportList)
        {
            dynamic expandoObject = new ExpandoObject();
            var supplierStats = stats.Stats as FileCalculateStatistics_Supplier;
            AddProperty(expandoObject, StatisticsReportHeaders.FieldName, SupplierStatisticsReportFieldNames.TotalRecords);
            AddProperty(expandoObject, StatisticsReportHeaders.Min, "");
            AddProperty(expandoObject, StatisticsReportHeaders.Max, supplierStats?.TotalRecords);
            exportList.Add(expandoObject);
        }

        private void GenerateInventoryStatsData(FileValidationStatsResult stats, List<dynamic> exportList)
        {
            dynamic expandoObject = new ExpandoObject();
            var inventoryStats = stats.Stats as FileCalculateStatistics_Inventory;
            AddProperty(expandoObject, StatisticsReportHeaders.FieldName, InventoryStatisticsReportFieldNames.TotalRecords);
            AddProperty(expandoObject, StatisticsReportHeaders.Min, "");
            AddProperty(expandoObject, StatisticsReportHeaders.Max, inventoryStats?.TotalRecords);
            exportList.Add(expandoObject);

            expandoObject = new ExpandoObject();
            AddProperty(expandoObject, StatisticsReportHeaders.FieldName, InventoryStatisticsReportFieldNames.Quantity);
            AddProperty(expandoObject, StatisticsReportHeaders.Min, inventoryStats?.QuantityMin);
            AddProperty(expandoObject, StatisticsReportHeaders.Max, inventoryStats?.QuantityMax);
            exportList.Add(expandoObject);

            expandoObject = new ExpandoObject();
            AddProperty(expandoObject, StatisticsReportHeaders.FieldName, InventoryStatisticsReportFieldNames.StandardCost);
            AddProperty(expandoObject, StatisticsReportHeaders.Min, inventoryStats?.StandardCostMin);
            AddProperty(expandoObject, StatisticsReportHeaders.Max, inventoryStats?.StandardCostMax);
            exportList.Add(expandoObject);

            expandoObject = new ExpandoObject();
            AddProperty(expandoObject, StatisticsReportHeaders.FieldName, InventoryStatisticsReportFieldNames.TotalValue);
            AddProperty(expandoObject, StatisticsReportHeaders.Min, inventoryStats?.TotalValueMin);
            AddProperty(expandoObject, StatisticsReportHeaders.Max, inventoryStats?.TotalValueMax);
            exportList.Add(expandoObject);

            expandoObject = new ExpandoObject();
            AddProperty(expandoObject, StatisticsReportHeaders.FieldName, InventoryStatisticsReportFieldNames.InventoryDate);
            AddProperty(expandoObject, StatisticsReportHeaders.Min, inventoryStats?.InventoryDateMin);
            AddProperty(expandoObject, StatisticsReportHeaders.Max, inventoryStats?.InventoryDateMax);

            exportList.Add(expandoObject);
        }

        private void GeneratePOStatsData(FileValidationStatsResult stats, List<dynamic> exportList)
        {
            dynamic expandoObject = new ExpandoObject();
            var poStats = stats.Stats as FileCalculateStatistics_PO;
            AddProperty(expandoObject, StatisticsReportHeaders.FieldName, POStatisticsReportFieldNames.TotalRecords);
            AddProperty(expandoObject, StatisticsReportHeaders.Min, "");
            AddProperty(expandoObject, StatisticsReportHeaders.Max, poStats?.TotalRecords);
            exportList.Add(expandoObject);

            expandoObject = new ExpandoObject();
            AddProperty(expandoObject, StatisticsReportHeaders.FieldName, POStatisticsReportFieldNames.OrderDateCost);
            AddProperty(expandoObject, StatisticsReportHeaders.Min, poStats?.OrderDateMin);
            AddProperty(expandoObject, StatisticsReportHeaders.Max, poStats?.OrderDateMax);
            exportList.Add(expandoObject);

            expandoObject = new ExpandoObject();
            AddProperty(expandoObject, StatisticsReportHeaders.FieldName, POStatisticsReportFieldNames.LatestAmendment);
            AddProperty(expandoObject, StatisticsReportHeaders.Min, poStats?.LatestAmendmentMin);
            AddProperty(expandoObject, StatisticsReportHeaders.Max, poStats?.LatestAmendmentMax);
            exportList.Add(expandoObject);
        }

        private void GeneratePOItemStatsData(FileValidationStatsResult stats, List<dynamic> exportList)
        {
            var poItemStats = stats.Stats as FileCalculateStatistics_POItem;

            dynamic expandoObject = new ExpandoObject();
            AddProperty(expandoObject, StatisticsReportHeaders.FieldName, POItemStatisticsReportFieldNames.TotalRecords);
            AddProperty(expandoObject, StatisticsReportHeaders.Min, "");
            AddProperty(expandoObject, StatisticsReportHeaders.Max, poItemStats?.TotalRecords);
            exportList.Add(expandoObject);

            expandoObject = new ExpandoObject();
            AddProperty(expandoObject, StatisticsReportHeaders.FieldName, POItemStatisticsReportFieldNames.UnitCost);
            AddProperty(expandoObject, StatisticsReportHeaders.Min, poItemStats?.UnitCostMin);
            AddProperty(expandoObject, StatisticsReportHeaders.Max, poItemStats?.UnitCostMax);
            exportList.Add(expandoObject);

            expandoObject = new ExpandoObject();
            AddProperty(expandoObject, StatisticsReportHeaders.FieldName, POItemStatisticsReportFieldNames.OrderedValue);
            AddProperty(expandoObject, StatisticsReportHeaders.Min, poItemStats?.OrderedValueMin);
            AddProperty(expandoObject, StatisticsReportHeaders.Max, poItemStats?.OrderedValueMax);
            exportList.Add(expandoObject);

            expandoObject = new ExpandoObject();
            AddProperty(expandoObject, StatisticsReportHeaders.FieldName, POItemStatisticsReportFieldNames.QuantityOrdered);
            AddProperty(expandoObject, StatisticsReportHeaders.Min, poItemStats?.QuantityOrderedMin);
            AddProperty(expandoObject, StatisticsReportHeaders.Max, poItemStats?.QuantityOrderedMax);
            exportList.Add(expandoObject);

            expandoObject = new ExpandoObject();
            AddProperty(expandoObject, StatisticsReportHeaders.FieldName, POItemStatisticsReportFieldNames.QuantityReturned);
            AddProperty(expandoObject, StatisticsReportHeaders.Min, poItemStats?.QuantityReturnedMin);
            AddProperty(expandoObject, StatisticsReportHeaders.Max, poItemStats?.QuantityReturnedMax);

            expandoObject = new ExpandoObject();
            AddProperty(expandoObject, StatisticsReportHeaders.FieldName, POItemStatisticsReportFieldNames.CommittedDate);
            AddProperty(expandoObject, StatisticsReportHeaders.Min, poItemStats?.CommittedDateMin);
            AddProperty(expandoObject, StatisticsReportHeaders.Max, poItemStats?.CommittedDateMax);

            expandoObject = new ExpandoObject();
            AddProperty(expandoObject, StatisticsReportHeaders.FieldName, POItemStatisticsReportFieldNames.RequestedDate);
            AddProperty(expandoObject, StatisticsReportHeaders.Min, poItemStats?.RequestedDateMin);
            AddProperty(expandoObject, StatisticsReportHeaders.Max, poItemStats?.RequestedDateMax);

            expandoObject = new ExpandoObject();
            AddProperty(expandoObject, StatisticsReportHeaders.FieldName, POItemStatisticsReportFieldNames.QtyLeftToReceive);
            AddProperty(expandoObject, StatisticsReportHeaders.Min, poItemStats?.QtyLeftToReceiveMin);
            AddProperty(expandoObject, StatisticsReportHeaders.Max, poItemStats?.QtyLeftToReceiveMax);

            expandoObject = new ExpandoObject();
            AddProperty(expandoObject, StatisticsReportHeaders.FieldName, POItemStatisticsReportFieldNames.ValueLeftToReceive);
            AddProperty(expandoObject, StatisticsReportHeaders.Min, poItemStats?.ValueLeftToReceiveMin);
            AddProperty(expandoObject, StatisticsReportHeaders.Max, poItemStats?.ValueLeftToReceiveMax);

            exportList.Add(expandoObject);
        }

        private void GenerateUOMStatsData(FileValidationStatsResult stats, List<dynamic> exportList)
        {
            dynamic expandoObject = new ExpandoObject();
            var uomStats = stats.Stats as FileCalculateStatistics_UOM;
            AddProperty(expandoObject, StatisticsReportHeaders.FieldName, UOMStatisticsReportFieldNames.TotalRecords);
            AddProperty(expandoObject, StatisticsReportHeaders.Min, "");
            AddProperty(expandoObject, StatisticsReportHeaders.Max, uomStats?.TotalRecords);
            exportList.Add(expandoObject);

            expandoObject = new ExpandoObject();
            AddProperty(expandoObject, StatisticsReportHeaders.FieldName, UOMStatisticsReportFieldNames.ConversionRate);
            AddProperty(expandoObject, StatisticsReportHeaders.Min, uomStats?.ConversionRateMin);
            AddProperty(expandoObject, StatisticsReportHeaders.Max, uomStats?.ConversionRateMax);
            exportList.Add(expandoObject);
        }

        private void GenerateMPNStatsData(FileValidationStatsResult stats, List<dynamic> exportList)
        {
            dynamic expandoObject = new ExpandoObject();
            var mpnStats = stats.Stats as FileCalculateStatistics_MPN;
            AddProperty(expandoObject, StatisticsReportHeaders.FieldName, MPNStatisticsReportFieldNames.TotalRecords);
            AddProperty(expandoObject, StatisticsReportHeaders.Min, "");
            AddProperty(expandoObject, StatisticsReportHeaders.Max, mpnStats?.TotalRecords);
            exportList.Add(expandoObject);
        }
               
    }
}
