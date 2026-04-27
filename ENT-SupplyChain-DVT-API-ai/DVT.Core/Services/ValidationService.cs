using DVT.Core.FileLoader;
using DVT.Core.Helper;
using DVT.Core.Models;
using DVT.Core.Validators;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;
using static DVT.Core.Constants;

namespace DVT.Core.Services
{
    public class ValidationService : IValidationService
    {
        private readonly IJobService _jobService;
        private readonly IJobFileService _jobFileService;
        private readonly IFileLoadService _fileLoadService;
        private readonly IActivityLogService _activityLogService;
        private readonly IMasterDataService _masterDataService;
        private readonly ILogFileService _logFileService;
        private IEnumerable<MasterData> _masterData;


        public ValidationService(IMasterDataService masterDataService, IJobService jobService, IJobFileService jobFileService, IActivityLogService activityLogService, IFileLoadService fileLoadService, IStorageService storageService, IUserInfoService userInfoService, IOutputFileService outputFileService, ILogFileService logFileService)//, IRealtimeStatusReportService realtimeStatusReportService)
        {
            _masterDataService = masterDataService;
            _jobService = jobService;
            _jobFileService = jobFileService;
            _fileLoadService = fileLoadService;
            _activityLogService = activityLogService;
            _logFileService = logFileService;
            
            _masterData = _masterDataService.GetAllMasterDataAsync().Result;
        }

        /// <summary>
        /// User Story 16171129: 4 - Job Controller - Validate Files
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="selectedFileIds"></param>
        /// <param name="userEmail"></param>
        /// <returns></returns>
        public async ValueTask<OperationResult> ValidateFilesAsync(Guid jobId, List<Guid> selectedFileIds, string userEmail)
        {
            try
            {
                StopwatchLogger logger = new StopwatchLogger("ValidateFilesAsync");
                logger.Start();

                var prepareJobResult = await PrepareJobForValidationAsync(jobId, selectedFileIds, userEmail);

                logger.StopAndLog("PrepareJob", true);                

                if (!prepareJobResult.Success)
                {
                    return prepareJobResult;
                }

                var job = (Job)prepareJobResult.Data;

                var jobFiles = job.JobFiles;

                var selectedFiles = jobFiles.Where(f => selectedFileIds.Contains(f.JobFileId)).ToList();

                //If no files selected, return
                if (selectedFiles == null || selectedFiles.Count == 0)
                {
                    return new OperationResult
                    {
                        Operation = Operations.ValidateFiles,
                        Success = false,
                        Message = StardardMessages.NoFilesSelectedForValidation
                    };
                }
                
                var utcNow = DateTime.UtcNow;

                var jobModelResult = await LoadAndPrepareJobModelAsync(job, selectedFiles, userEmail);

                if (!jobModelResult.Success)
                {
                    return jobModelResult;
                }

                var jobModel = (JobModel)jobModelResult.Data;

                if (jobModel == null || jobModel.JobFiles == null || !jobModel.JobFiles.Any())
                {
                    return new OperationResult
                    {
                        Operation = Operations.ValidateFiles,
                        Success = false,
                        Message = StardardMessages.NoFilesLoadedForValidation
                    };
                }

                logger.StopAndLog("LoadAndPrepareJobModel", true);                

                //Call the validation service to validate the files.
                var jobValidationResult = await ExecuteValidationAsync(jobModel);

                logger.StopAndLog("ExecuteValidation", true);
                
                if (jobValidationResult.Success)
                {
                    job.Status = WellKnownJobStatuses.Validated;
                    
                    var jobAllFiles = await _jobFileService.GetJobFilesByJobIdAsync(jobId);
                    var validatedFiles = jobAllFiles.Where(f => selectedFileIds.Contains(f.JobFileId)).ToList();

                    logger.StopAndLog("GetJobFiles", true);
                    
                    //Create related files in user's log folder
                    await _logFileService.CreateLogFilesAsync(job, validatedFiles, userEmail);

                    logger.StopAndLog("CreateLogFiles", true);
                }
                else
                {
                    job.Status = WellKnownJobStatuses.Failed;
                }

                utcNow = DateTime.UtcNow;

                job.UpdateBy = userEmail;
                job.UpdateDate = utcNow;
                await _jobService.UpdateJobStatusAsync(job.JobId, job.Status, userEmail);

                logger.StopAndLog("UpdateJobStatus", false);
                await AddLog(job.JobId, logger.Log.ToString(), "Timing", true);              

                return new OperationResult
                {
                    Operation = Operations.ValidateFiles,
                    Success = jobValidationResult.Success,
                    Data = job,
                    Message = jobValidationResult.Success ? StardardMessages.ValidationCompletedSuccessfully : jobValidationResult.ExceptionMessage
                };
            }
            catch (Exception ex)
            {
                AddLog(jobId, Operations.ValidateFiles + ", Exception Message: " + ex.Message, userEmail, true, LogMessageTypes.Error).Wait();
                return new OperationResult
                {
                    Operation = Operations.ValidateFiles,
                    Success = false,
                    Message = StardardMessages.ValidationFailed
                };
            }
        }

        private async Task<OperationResult> PrepareJobForValidationAsync(Guid jobId, List<Guid> selectedFileIds, string userEmail)
        {
            var result = new OperationResult
            {
                Operation = Operations.PrepareJob,
            };

            try
            {
                if (jobId == Guid.Empty)
                {
                    result.Success = false;
                    result.Message = StardardMessages.BadRequest;
                    return result;
                }

                if (selectedFileIds == null || selectedFileIds.Count == 0)
                {
                    result.Success = false;
                    result.Message = StardardMessages.NoFilesSelectedForValidation;
                    return result;
                }
                
                if (_masterData == null || !_masterData.Any())
                {
                    result.Success = false;
                    result.Message = StardardMessages.NoMasterData;
                    return result;
                }

                var job = await _jobService.GetJobByIdNoTrackingAsync(jobId);

                var jobFiles = await _jobFileService.GetJobFilesByJobIdNoValidationNoTrackingAsync(jobId);

                if (jobFiles == null || !jobFiles.Any())
                {
                    result.Success = false;
                    result.Message = StardardMessages.NoFilesUnderThisJob;
                    return result;
                }

                job.JobFiles = jobFiles.ToList();

                result.Success = true;
                result.Data = job;
                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = Operations.PrepareJob + " errors: " + ex.Message;
                result.Exception = new Exception(ex.Message);
                return result;
            }
        }

        private async Task<OperationResult> LoadAndPrepareJobModelAsync(Job job, List<JobFile> selectedFiles, string userEmail)
        {
            //Before Validate files - Change job and file status to IN_PROGRESS.
            await ChangeStatusAsync(job, WellKnownJobStatuses.InProgress, true, selectedFiles, WellKnownFileStatuses.InProgress, userEmail);

            var jobLoadResult = await LoadFilesAsync(job, selectedFiles, userEmail);

            if (jobLoadResult.FileLoadResults != null && jobLoadResult.FileLoadResults != null && jobLoadResult.FileLoadResults.Any() && jobLoadResult.FileLoadResults.Any(f => !f.Success))
            {
                if (jobLoadResult.FileLoadResults.All(f => !f.Success))
                {
                    await ChangeStatusAsync(job, WellKnownJobStatuses.Failed, true, selectedFiles, WellKnownFileStatuses.Failed, userEmail);

                    return new OperationResult
                    {
                        Operation = Operations.ValidateFiles,
                        Success = false,
                        Message = StardardMessages.NoFilesLoadedForValidation,
                        Exception = new Exception(jobLoadResult.FileLoadResults.Select(x => x.Message).First())
                    };
                }
                else
                {
                    await _jobFileService.BatchUpdateJobFilesStatusAsync(selectedFiles.Where(x => jobLoadResult.FileLoadResults.Where(f => !f.Success).Select(f => f.JobFileId).Contains(x.JobFileId)).Select(x => x.JobFileId).ToList(), WellKnownFileStatuses.Failed, userEmail);
                }
            }

            //parpare the files for validation
            var jobModel = GetJobModelFromJobAndJobLoadResult(job, jobLoadResult, selectedFiles, userEmail);

            if (jobModel == null || jobModel.JobFiles == null || !jobModel.JobFiles.Any())
            {
                return new OperationResult
                {
                    Operation = Operations.ValidateFiles,
                    Success = false,
                    Message = StardardMessages.NoFilesLoadedForValidation
                };
            }
            else
            {
                return new OperationResult
                {
                    Operation = Operations.ValidateFiles,
                    Success = true,
                    Data = jobModel
                };
            }
        }

        private async Task ChangeStatusAsync(Job job, string jobStatus, bool changeFiles, List<JobFile> jobFiles, string fileStatus, string updateBy)
        {
            var utcNow = DateTime.UtcNow;
            job.Status = jobStatus;
            job.UpdateBy = updateBy;
            job.UpdateDate = utcNow;

            await _jobService.UpdateJobStatusAsync(job.JobId, job.Status, updateBy);

            if (changeFiles && jobFiles.Any())
            {
                await _jobFileService.BatchUpdateJobFilesStatusAsync(jobFiles.Select(x => x.JobFileId).ToList(), fileStatus, updateBy);
            }

            //Send the job status update via SignalR
            var jobStatusUpdate = new JobStatusUpdate(job.JobId);
            foreach (var file in jobFiles)
            {
                jobStatusUpdate.AddFileStatus(file.JobFileId, fileStatus);
            }
            //await _realtimeStatusReportService.SendJobStatusUpdate(jobStatusUpdate);
        }

        private async ValueTask<JobLoadResult> LoadFilesAsync(Job job, IEnumerable<JobFile> jobFiles, string userEmail)
        {
            var jobLoadResult = await _fileLoadService.LoadFile(new JobLoad
            {
                JobId = job.JobId,
                DivisionId = job.DivisionId,
                FeedNumber = job.FeedNumber,
                UserEmail = userEmail,
                FileList = jobFiles.Select(file => new FileLoadRequest
                {
                    JobFileId = file.JobFileId,
                    JobId = file.JobId,
                    FileName = file.FileName,
                    FilePath = file.FilePath,
                    FileType = file.FileType
                }).ToList()
            });

            return jobLoadResult;
        }

        /// <summary>
        /// This method takes the job from the database and the data rows from the file load result and creates
        /// a JobModel which contains the data rows from the loaded files so that we can validate them.
        /// </summary>
        /// <param name="job">Job information from the database</param>
        /// <param name="jobLoadResult">Result of loading the flat-text files into a data model</param>
        /// <returns></returns>
        private JobModel GetJobModelFromJobAndJobLoadResult(Job job, JobLoadResult jobLoadResult, IEnumerable<JobFile> selectedFiles, string updateBy)
        {
            var jobModel = new JobModel
            {
                JobId = job.JobId,
                DivisionId = job.DivisionId,
                FeedNumber = job.FeedNumber,
                UserInfoId = job.UserInfoId,
                ArchiveFilePath = job.ArchiveFilePath,
                Status = job.Status,
                UpdateBy = updateBy,
                JobFiles = new List<IJobFileModel>()
            };

            FileLoadResult fileLoadResult = null;
            FileDependencyLoader fileDependencyLoader = new FileDependencyLoader();
            //Convert the job files to the model
            foreach (var file in selectedFiles)
            {
                fileLoadResult = jobLoadResult.GetFileLoadResultByJobFileId(file.JobFileId);

                if (fileLoadResult == null)
                {
                    continue;
                }

                if (!fileLoadResult.Success)
                {
                    continue;
                }

                var dataRows = fileLoadResult.DataRows;

                var fileHeader = fileLoadResult.FileHeader;

                var fileModel = new JobFileModel
                {
                    JobFileId = file.JobFileId,
                    FileType = file.FileType,
                    FileName = file.FileName,
                    IsSelected = true,
                    //ValidationMessages = file.ValidationMessages,
                    FileHeader = fileHeader,
                    DataRows = dataRows
                };
                jobModel.JobFiles.Add(fileModel);

                //Load dependent files if any
                fileDependencyLoader.LoadDependentFiles(job, jobModel, jobLoadResult, file, updateBy);
            }
            return jobModel;
        }

        private async Task<JobValidationResult> ExecuteValidationAsync(JobModel job)
        {
            JobValidationResult jobValidationResult = new JobValidationResult();
            jobValidationResult.Success = true;
            var errorSB = new StringBuilder();
            var dataRowsCount = 0;

            foreach (var file in job.JobFiles)
            {
                try
                {
                    dataRowsCount = 0;

                    if (file == null)
                    {
                        continue;
                    }
                    if (!file.IsSelected)
                    {
                        continue;
                    }
                    if (file.DataRows == null || !file.DataRows.Any())
                    {
                        AddLog(file.JobFileId, Operations.ValidateFiles + ", " + ValidationMessages.TheFileNoDataRow + " " + file.FileName, job.UpdateBy).Wait();
                    }

                    dataRowsCount = file.DataRows.Count;

                    AddLog(file.JobFileId, Operations.ValidateFiles + ", " + string.Format(ValidationMessages.ValidateFile, file.FileType, file.FileName), job.UpdateBy).Wait();

                    if (string.Equals(file.FileType, Constants.FileTypes.Vir, StringComparison.OrdinalIgnoreCase))
                    {
                        await ValidateVirFile(job, file, _masterData, dataRowsCount, jobValidationResult);
                    }
                    else if (string.Equals(file.FileType, Constants.FileTypes.Item, StringComparison.OrdinalIgnoreCase))
                    {
                        await ValidateItemFile(job, file, _masterData, dataRowsCount, jobValidationResult);
                    }
                    else if (string.Equals(file.FileType, Constants.FileTypes.Supplier, StringComparison.OrdinalIgnoreCase))
                    {
                        await ValidateSupplierFile(job, file, _masterData, dataRowsCount, jobValidationResult);
                    }
                    else if (string.Equals(file.FileType, Constants.FileTypes.Inventory, StringComparison.OrdinalIgnoreCase))
                    {
                        await ValidateInventoryFile(job, file, _masterData, dataRowsCount, jobValidationResult);
                    }
                    else if (string.Equals(file.FileType, Constants.FileTypes.Po, StringComparison.OrdinalIgnoreCase))
                    {
                        await ValidatePOFile(job, file, _masterData, dataRowsCount, jobValidationResult);
                    }
                    else if (string.Equals(file.FileType, Constants.FileTypes.PoItem, StringComparison.OrdinalIgnoreCase))
                    {
                        await ValidatePOItemFile(job, file, _masterData, dataRowsCount, jobValidationResult);
                    }
                    else if (string.Equals(file.FileType, Constants.FileTypes.Uom, StringComparison.OrdinalIgnoreCase))
                    {
                        await ValidateUOMFile(job, file, _masterData, dataRowsCount, jobValidationResult);
                    }
                    else if (string.Equals(file.FileType, Constants.FileTypes.Mpn, StringComparison.OrdinalIgnoreCase))
                    {
                        await ValidateMPNFile(job, file, _masterData, dataRowsCount, jobValidationResult);
                    }                    
                }
                catch (Exception ex)
                {
                    await _jobFileService.UpdateJobFileStatusAsync(file.JobFileId, WellKnownFileStatuses.Failed, job.UpdateBy);

                    AddLog(file.JobFileId, Operations.ValidateFiles + ", " + string.Format(ValidationMessages.ValidateFileError, file.FileType, file.FileName, ex.Message), job.UpdateBy).Wait();
                    jobValidationResult.Success = false;
                    errorSB.Append(ex.Message);
                }

                //Send the job status update via SignalR
                // await _realtimeStatusReportService.SendJobStatusUpdate(jobStatusUpdate);
            }

            if (errorSB.Length != 0)
            {
                jobValidationResult.ExceptionMessage = errorSB.ToString();
            }

            AddLog(job.JobId, Operations.ValidateFiles + " " + (jobValidationResult.Success ? StardardMessages.ValidationCompletedSuccessfully : (StardardMessages.ValidationFailed + ", Errors: " + jobValidationResult.ExceptionMessage)), job.UpdateBy, true).Wait();

            return jobValidationResult;
        }

        private async Task AddLog(Guid entityId, string message, string updateBy, bool isJob = false, string messageType = "")
        {
            await _activityLogService.AddLogAsync(new ActivityLog
            {
                LogId = Guid.NewGuid(),
                EntityId = entityId,
                Entity = isJob ? DVTEntities.Job : DVTEntities.JobFile,
                MessageType = string.IsNullOrEmpty(messageType) ? LogMessageTypes.Info : messageType,
                Message = message,
                CreateBy = updateBy,
                CreateDate = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Task 19332404: 0 - Main Screen - Grid - Status Column (After Validation Process, Before Sending to Server) - enhancement
        /// </summary>
        /// <param name="validationResult"></param>
        /// <returns></returns>
        private string GetJobFileStatus(FileValidationResult validationResult)
        {
            var status = WellKnownFileStatuses.Validated;
            if (validationResult.RowValidationResults != null && validationResult.RowValidationResults.Any())
            {
                if (validationResult.RowValidationResults.Any(v => v.ValidationResult.Errors.Any(r => string.Equals(r.ErrorCode, WellKnownFileStatuses.Warning, StringComparison.OrdinalIgnoreCase))))
                {
                    status = WellKnownFileStatuses.Warning;
                }

                if (validationResult.RowValidationResults.Any(v => v.ValidationResult.Errors.Any(r => string.Equals(r.ErrorCode, WellKnownFileStatuses.Errors, StringComparison.OrdinalIgnoreCase))))
                {
                    status = WellKnownFileStatuses.Errors;
                }

                if (validationResult.RowValidationResults.Any(v => v.ValidationResult.Errors.Any(r => string.Equals(r.ErrorCode, WellKnownFileStatuses.Critical, StringComparison.OrdinalIgnoreCase))))
                {
                    status = WellKnownFileStatuses.Critical;
                }
            }

            return status;
        }

        private async Task ValidateVirFile(JobModel job, IJobFileModel file, IEnumerable<MasterData> masterData, int dataRowsCount, JobValidationResult jobValidationResult)
        {
            StopwatchLogger logger = new StopwatchLogger($"ValidateVirFile", dataRowsCount);
            logger.Start();

            var virFileValidator = new VirFileValidator();
            var fileValidationResult = virFileValidator.ValidateAsync(job, file, masterData);

            logger.AppendToLog(fileValidationResult.AdditionalInfo);
            logger.StopAndLog("Validation Complete", true);

            var statistics = virFileValidator.GetFileCalculateStatistics(file, dataRowsCount);

            logger.StopAndLog("CalculateStats", true);

            jobValidationResult.FileValidationErrors.Add(fileValidationResult);
            var status = GetJobFileStatus(fileValidationResult);

            logger.StopAndLog("GetJobFileStatus", true);

            var validationMessage = JsonSerializer.Serialize(fileValidationResult.RowValidationResults);
            var statisticsMessage = JsonSerializer.Serialize(statistics);

            logger.StopAndLog("Serialize", true);
            await _jobFileService.UpdateJobFileValidationResultAsync(file.JobFileId, validationMessage, statisticsMessage, status, job.UpdateBy);

            logger.StopAndLog("UpdateJobFile", false);
            await AddLog(file.JobFileId, logger.Log.ToString(), "Timing", false);
        }

        private async Task ValidateItemFile(JobModel job, IJobFileModel file, IEnumerable<MasterData> masterData, int dataRowsCount, JobValidationResult jobValidationResult)
        {
            StopwatchLogger logger = new StopwatchLogger("ValidateItemFile", dataRowsCount);
            logger.Start();

            var itemFileValidator = new ItemFileValidator();
            var fileValidationResult = itemFileValidator.ValidateAsync(job, file, masterData);

            logger.Log.AppendLine(fileValidationResult.AdditionalInfo);
            logger.StopAndLog("Validation Complete", true);

            var statistics = itemFileValidator.GetFileCalculateStatistics(file, dataRowsCount);

            logger.StopAndLog("CalculateStats", true);

            jobValidationResult.FileValidationErrors.Add(fileValidationResult);
            var status = GetJobFileStatus(fileValidationResult);

            logger.StopAndLog("GetJobFileStatus", true);

            var validationMessage = JsonSerializer.Serialize(fileValidationResult.RowValidationResults);
            var statisticsMessage = JsonSerializer.Serialize(statistics);

            logger.StopAndLog("Serialize", true);

            await _jobFileService.UpdateJobFileValidationResultAsync(file.JobFileId, validationMessage, statisticsMessage, status, job.UpdateBy);

            logger.StopAndLog("UpdateJobFile", false);
            await AddLog(file.JobFileId, logger.Log.ToString(), "Timing", false);
        }

        private async Task ValidateSupplierFile(JobModel job, IJobFileModel file, IEnumerable<MasterData> masterData, int dataRowsCount, JobValidationResult jobValidationResult)
        {
            StopwatchLogger logger = new StopwatchLogger("ValidateSupplierFile", dataRowsCount);
            logger.Start();

            var supplierFileValidator = new SupplierFileValidator();
            var fileValidationResult = supplierFileValidator.ValidateAsync(job, file, masterData);

            logger.AppendToLog(fileValidationResult.AdditionalInfo);
            logger.StopAndLog("Validation Complete", true);

            var statistics = supplierFileValidator.GetFileCalculateStatistics(file, dataRowsCount);

            logger.StopAndLog("CalculateStatistics", true); 

            jobValidationResult.FileValidationErrors.Add(fileValidationResult);
            var status = GetJobFileStatus(fileValidationResult);

            logger.StopAndLog("GetJobFileStatus", true);

            var validationMessage = JsonSerializer.Serialize(fileValidationResult.RowValidationResults);
            var statisticsMessage = JsonSerializer.Serialize(statistics);

            logger.StopAndLog("Serialize", true);

            await _jobFileService.UpdateJobFileValidationResultAsync(file.JobFileId, validationMessage, statisticsMessage, status, job.UpdateBy);

            logger.StopAndLog("UpdateJobFile", false);
            await AddLog(file.JobFileId, logger.Log.ToString(), "Timing", false);
        }

        private async Task ValidateInventoryFile(JobModel job, IJobFileModel file, IEnumerable<MasterData> masterData, int dataRowsCount, JobValidationResult jobValidationResult)
        {
            StopwatchLogger logger = new StopwatchLogger("ValidateInventoryFile", dataRowsCount);
            logger.Start();

            var inventoryFileValidator = new InventoryFileValidator();
            var fileValidationResult = inventoryFileValidator.ValidateAsync(job, file, masterData);

            logger.AppendToLog(fileValidationResult.AdditionalInfo);
            logger.StopAndLog("Validation Complete", true);

            var statistics = inventoryFileValidator.GetFileCalculateStatistics(file, dataRowsCount);

            logger.StopAndLog("CalculateStats", true);

            jobValidationResult.FileValidationErrors.Add(fileValidationResult);
            var status = GetJobFileStatus(fileValidationResult);

            logger.StopAndLog("GetJobFileStatus", true);

            var validationMessage = JsonSerializer.Serialize(fileValidationResult.RowValidationResults);
            var statisticsMessage = JsonSerializer.Serialize(statistics);

            logger.StopAndLog("Serialize", true);

            await _jobFileService.UpdateJobFileValidationResultAsync(file.JobFileId, validationMessage, statisticsMessage, status, job.UpdateBy);

            logger.StopAndLog("UpdateJobFile", false);
            await AddLog(file.JobFileId, logger.Log.ToString(), "Timing", false);
        }

        private async Task ValidatePOFile(JobModel job, IJobFileModel file, IEnumerable<MasterData> masterData, int dataRowsCount, JobValidationResult jobValidationResult)
        {
            StopwatchLogger logger = new StopwatchLogger("ValidatePOFile", dataRowsCount);
            logger.Start();

            var poFileValidator = new POFileValidator();
            var fileValidationResult = poFileValidator.ValidateAsync(job, file, masterData);

            logger.AppendToLog(fileValidationResult.AdditionalInfo);
            logger.StopAndLog("Validation Complete", true);

            var statistics = poFileValidator.GetFileCalculateStatistics(file, dataRowsCount);

            logger.StopAndLog("CalculateStats", true);

            jobValidationResult.FileValidationErrors.Add(fileValidationResult);
            var status = GetJobFileStatus(fileValidationResult);

            logger.StopAndLog("GetJobFileStatus", true);

            var validationMessage = JsonSerializer.Serialize(fileValidationResult.RowValidationResults);
            var statisticsMessage = JsonSerializer.Serialize(statistics);

            logger.StopAndLog("Serialize", true);

            await _jobFileService.UpdateJobFileValidationResultAsync(file.JobFileId, validationMessage, statisticsMessage, status, job.UpdateBy);

            logger.StopAndLog("UpdateJobFile", false);
            await AddLog(file.JobFileId, logger.Log.ToString(), "Timing", false);
        }

        private async Task ValidatePOItemFile(JobModel job, IJobFileModel file, IEnumerable<MasterData> masterData, int dataRowsCount, JobValidationResult jobValidationResult)
        {
            StopwatchLogger logger = new StopwatchLogger("ValidatePOItemFile", dataRowsCount);
            logger.Start();

            var poItemFileValidator = new POItemFileValidator();
            var fileValidationResult = poItemFileValidator.ValidateAsync(job, file, masterData);

            logger.AppendToLog(fileValidationResult.AdditionalInfo);
            logger.StopAndLog("Validation Complete", true);

            jobValidationResult.FileValidationErrors.Add(fileValidationResult);
            var status = GetJobFileStatus(fileValidationResult);

            logger.StopAndLog("GetJobFileStatus", true);

            var statistics = poItemFileValidator.GetFileCalculateStatistics(file, dataRowsCount);

            logger.StopAndLog("CalculateStats", true);
            
            var validationMessage = JsonSerializer.Serialize(fileValidationResult.RowValidationResults);
            var statisticsMessage = JsonSerializer.Serialize(statistics);

            logger.StopAndLog("Serialize", true);
            
            await _jobFileService.UpdateJobFileValidationResultAsync(file.JobFileId, validationMessage, statisticsMessage, status, job.UpdateBy);

            logger.StopAndLog("UpdateJobFile", false);
            await AddLog(file.JobFileId, logger.Log.ToString(), "Timing", false);
        }

        private async Task ValidateUOMFile(JobModel job, IJobFileModel file, IEnumerable<MasterData> masterData, int dataRowsCount, JobValidationResult jobValidationResult)
        {
            StopwatchLogger logger = new StopwatchLogger("ValidateUOMFile", dataRowsCount);
            logger.Start();

            var uomFileValidator = new UOMFileValidator();
            var fileValidationResult = uomFileValidator.ValidateAsync(job, file, masterData);

            logger.AppendToLog(fileValidationResult.AdditionalInfo);
            logger.StopAndLog("Validation Complete", true);

            jobValidationResult.FileValidationErrors.Add(fileValidationResult);
            var status = GetJobFileStatus(fileValidationResult);

            logger.StopAndLog("GetJobFileStatus", true);

            var statistics = uomFileValidator.GetFileCalculateStatistics(file, dataRowsCount);

            logger.StopAndLog("CalculateStats", true);

            var validationMessage = JsonSerializer.Serialize(fileValidationResult.RowValidationResults);
            var statisticsMessage = JsonSerializer.Serialize(statistics);

            logger.StopAndLog("Serialize", true);

            await _jobFileService.UpdateJobFileValidationResultAsync(file.JobFileId, validationMessage, statisticsMessage, status, job.UpdateBy);

            logger.StopAndLog("UpdateJobFile", false);
            await AddLog(file.JobFileId, logger.Log.ToString(), "Timing", false);
        }
        
        private async Task ValidateMPNFile(JobModel job, IJobFileModel file, IEnumerable<MasterData> masterData, int dataRowsCount, JobValidationResult jobValidationResult)
        {
            StopwatchLogger logger = new StopwatchLogger("ValidateMPNFile", dataRowsCount);
            logger.Start();

            var mpnFileValidator = new MPNFileValidator();
            var fileValidationResult = mpnFileValidator.ValidateAsync(job, file, masterData);

            logger.AppendToLog(fileValidationResult.AdditionalInfo);
            logger.StopAndLog("Validation Complete", true);

            jobValidationResult.FileValidationErrors.Add(fileValidationResult);
            var status = GetJobFileStatus(fileValidationResult);

            logger.StopAndLog("GetJobFileStatus", true);

            var statistics = mpnFileValidator.GetFileCalculateStatistics(file, dataRowsCount);

            logger.StopAndLog("CalculateStats", true);

            var validationMessage = JsonSerializer.Serialize(fileValidationResult.RowValidationResults);
            var statisticsMessage = JsonSerializer.Serialize(statistics);

            logger.StopAndLog("Serialize", true);

            await _jobFileService.UpdateJobFileValidationResultAsync(file.JobFileId, validationMessage, statisticsMessage, status, job.UpdateBy);

            logger.StopAndLog("UpdateJobFile", false);
            await AddLog(file.JobFileId, logger.Log.ToString(), "Timing", false);
        }
        
    }
}
