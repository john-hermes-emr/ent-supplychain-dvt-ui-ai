using DVT.Core.Models;
using FluentValidation;
using Newtonsoft.Json;
using static DVT.Core.Constants;

namespace DVT.Core.Services
{
    public class JobService : IJobService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStorageService _storageService;
        private readonly IActivityLogService _activityLogService;
        private readonly IMasterDataService _masterDataService;
        private readonly IUserInfoService _userInfoService;
        private readonly IValidator<Job> _jobValidator;
        private readonly IJobFileService _jobFileService;

        public JobService(IUnitOfWork unitOfWork, IFileLoadService fileLoadService, IStorageService storageService, IActivityLogService activityLogService, IMasterDataService masterDataService, IUserInfoService userInfoService, IValidator<Job> jobValidator, IJobFileService jobFileService)//, IRealtimeStatusReportService realtimeStatusReportService)
        {
            _unitOfWork = unitOfWork;
            _storageService = storageService;
            _activityLogService = activityLogService;
            _masterDataService = masterDataService;
            _userInfoService = userInfoService;
            _jobValidator = jobValidator;
            _jobFileService = jobFileService;
            //_realtimeStatusReportService = realtimeStatusReportService;
        }

        /// <summary>
        /// User Story 16176788: 2 - Job Service - Create a Job
        /// User Story 15989157: 3 - Job Service - Orchestration
        /// User Story 19004543: 1 - VIR - Job Service Enhancement
        /// </summary>
        /// <param name="job"></param>
        /// <param name="forceCreate"></param>
        /// <returns></returns>
        /// <exception cref="ValidationException"></exception>
        public async ValueTask<OperationResult> CreateJobAsync(Job job, bool forceCreate)
        {
            var rtnResult = new OperationResult
            {
                Operation = Operations.CreateJob,
                Success = false,
            };

            var userInfo = await GetUserInfoAndCheckDirectory(job.UserInfoId);
            var userEmailAddress = userInfo.EmailAddress;

            var latestActiveJob = await GetUserLatestActiveJobAsync(job.UserInfoId);

            var mapper = new Mapping();
            if (latestActiveJob != null)
            {
                var activeJobDivId = latestActiveJob.DivisionId;
                var activeJobFeedNum = latestActiveJob.FeedNumber;

                if (activeJobDivId == job.DivisionId && activeJobFeedNum == job.FeedNumber)
                {
                    if (!forceCreate)
                    {
                        rtnResult.Message = StardardMessages.ActiveJobAlreadyExists;

                        if (latestActiveJob.JobFiles != null && latestActiveJob.JobFiles.Count > 0)
                        {
                            latestActiveJob.JobFiles.ForEach(file =>
                            {
                                file.ValidationMessages = null;
                                file.ValidationStats = null;
                            });
                        }

                        rtnResult.Data = mapper.JobToJobDto(latestActiveJob);
                        return rtnResult;
                    }
                    else
                    {
                        //delete the existing active job and create a new one.
                        await DeleteJobAsync(latestActiveJob.JobId, userEmailAddress);
                    }
                }
                else
                {
                    if (!forceCreate)
                    {
                        rtnResult.Message = StardardMessages.ExistingJobDoesNotMatchSelections;
                        if (latestActiveJob.JobFiles != null && latestActiveJob.JobFiles.Count > 0)
                        {
                            latestActiveJob.JobFiles.ForEach(file =>
                            {
                                file.ValidationMessages = null;
                                file.ValidationStats = null;
                            });
                        }
                        rtnResult.Data = mapper.JobToJobDto(latestActiveJob);
                        return rtnResult;
                    }
                    else
                    {
                        //delete the existing active job and create a new one.
                        await DeleteJobAsync(latestActiveJob.JobId, userEmailAddress);
                    }
                }
            }

            Exception exception = null;

            var message = "";
            var utcNow = DateTime.UtcNow;

            var jobId = Guid.NewGuid();

            var activityLogs = new List<ActivityLog>();

            var divisionId = job.DivisionId;
            var division = await _masterDataService.GetByIdAsync(divisionId);

            if (division == null)
            {
                rtnResult.Message = StardardMessages.InvalidDivision;
                return rtnResult;
            }

            try
            {
                var divisionAbbrev = division.ItemNameAbbrev;
                var feedNumber = job.FeedNumber;
                job.JobId = jobId;
                job.Status = WellKnownJobStatuses.New;
                job.CreateDate = utcNow;
                job.UpdateDate = utcNow;
                job.CreateBy = userEmailAddress;
                job.UpdateBy = userEmailAddress;

                var loadFolder = userInfo.LoadFolder;
                //get files info from load directory.
                var files = await GetJobFilesAsync(userEmailAddress, loadFolder, jobId, divisionAbbrev, feedNumber, activityLogs);

                if (files == null || files.Count == 0)
                {
                    throw new Exception(string.Format(StardardMessages.NoFilesFoundInDirectory, userInfo.LoadFolder));
                }

                job.JobFiles = files;

                var validationResult = await _jobValidator.ValidateAsync(job);

                if (!validationResult.IsValid)
                {
                    var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                    throw new ValidationException(StardardMessages.ValidationFailedMsg + errors);
                }

                await _unitOfWork.Jobs.AddAsync(job);
                await _unitOfWork.CommitAsync();

                message = StardardMessages.JobCreatedSuccessfully;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                exception = new Exception(ex.Message);
                activityLogs.Add(new ActivityLog
                {
                    LogId = Guid.NewGuid(),
                    EntityId = jobId,
                    Entity = DVTEntities.Job,
                    MessageType = LogMessageTypes.Error,
                    Message = message,
                    CreateBy = userEmailAddress,
                    CreateDate = utcNow
                });

                rtnResult.Exception = exception;
                rtnResult.Message = message;
                return rtnResult;
            }

            activityLogs.Insert(0, new ActivityLog
            {
                LogId = Guid.NewGuid(),
                EntityId = jobId,
                Entity = DVTEntities.Job,
                Message = message,
                CreateBy = userEmailAddress,
                CreateDate = utcNow
            });

            await _activityLogService.AddLogsAsync(activityLogs);

            rtnResult.Operation = Operations.CreateJob;
            rtnResult.Success = true;
            rtnResult.Message = message;
            rtnResult.Data = mapper.JobToJobDto(job);

            return rtnResult;
        }

        /// <summary>
        /// User Story 16176790: 6 - Job Service - Update a Job
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="KeyNotFoundException"></exception>
        public async ValueTask<OperationResult> UpdateJobAsync(Job job)
        {
            var result = new OperationResult
            {
                Operation = Operations.UpdateJob,
            };

            try
            {
                if (job == null)
                {
                    throw new ArgumentNullException(nameof(job), StardardMessages.ObjectCannotBeNull);
                }

                var existingJob = await _unitOfWork.Jobs.GetByIdAsync(job.JobId);
                if (existingJob == null)
                {
                    throw new KeyNotFoundException($"{StardardMessages.ItemNotFound} id: {job.JobId}");
                }

                existingJob.DivisionId = job.DivisionId;
                existingJob.FeedNumber = job.FeedNumber;
                existingJob.Status = job.Status;
                existingJob.ArchiveFilePath = job.ArchiveFilePath;
                existingJob.UpdateDate = DateTime.UtcNow;
                existingJob.UpdateBy = job.UpdateBy;

                await _unitOfWork.CommitAsync();
                await _activityLogService.AddLogAsync(new ActivityLog
                {
                    LogId = Guid.NewGuid(),
                    EntityId = job.JobId,
                    Entity = DVTEntities.Job,
                    Message = Operations.UpdateJob + " to " + job.Status,
                    CreateBy = job.UpdateBy,
                    CreateDate = DateTime.UtcNow
                });

                result.Success = true;
                result.Message = StardardMessages.JobUpdatedSuccessfully;
                result.Data = existingJob;
            }
            catch (Exception ex)
            {
                result.Success = true;
                result.Message = StardardMessages.JobUpdatedFailed;
                result.Exception = new Exception(ex.Message);
                await _activityLogService.AddLogAsync(new ActivityLog
                {
                    LogId = Guid.NewGuid(),
                    EntityId = job.JobId,
                    Entity = DVTEntities.Job,
                    MessageType = LogMessageTypes.Error,
                    Message = Operations.UpdateJob + ", Exception Message: " + result.Message + ", " + ex.Message,
                    CreateBy = job.UpdateBy,
                    CreateDate = DateTime.UtcNow
                });
            }
            return result;
        }

        public async ValueTask<OperationResult> LoadExtractFilesAsync(Guid jobId, string updateBy)
        {
            var job = await GetJobWithJobFilesNoValidationByIdAsync(jobId);

            var result = new OperationResult
            {
                Operation = Operations.LoadJobFiles,
            };

            var message = StardardMessages.JobFilesLoadedSuccessfully;

            var activityLogs = new List<ActivityLog>();

            try
            {
                var utcNow = DateTime.UtcNow;

                var newStatusFiles = job.JobFiles.Where(x => x.Status == WellKnownFileStatuses.New || x.Status == WellKnownFileStatuses.Uploaded).ToList();

                if (newStatusFiles.Count == 0)
                {
                    message = StardardMessages.NoNewJobFilesToLoad;
                    result.Success = true;
                    result.Message = message;
                    result.Data = job;
                    return result;
                }

                var files = await _storageService.LoadExtractFilesAsync(job, newStatusFiles);

                job.Status = job.Status == WellKnownJobStatuses.New ? WellKnownJobStatuses.Uploaded : job.Status;
                job.UpdateDate = utcNow;
                job.UpdateBy = updateBy;

                foreach (var file in job.JobFiles)
                {
                    var fileInfo = files.First(x => x.FileName.Equals(file.FileName, StringComparison.OrdinalIgnoreCase));
                    file.Status = file.Status == WellKnownFileStatuses.New ? WellKnownFileStatuses.Uploaded : file.Status;
                    file.UpdateBy = updateBy;
                    file.UpdateDate = utcNow;
                    file.RecordCount = fileInfo.RecordCount;
                    file.LoadDate = utcNow;
                    file.FileCreationTimestamp = fileInfo.FileCreationTimestamp;
                    file.FileLastModifiedTimestamp = fileInfo.FileLastModifiedTimestamp;
                    if (fileInfo.Deleted)
                    {
                        file.Deleted = true;

                        activityLogs.Add(new ActivityLog
                        {
                            LogId = Guid.NewGuid(),
                            EntityId = file.JobFileId,
                            Entity = DVTEntities.JobFile,
                            MessageType = LogMessageTypes.Warning,
                            Message = string.Format(StardardMessages.FileMarkedDeleted, file.FileName),
                            CreateBy = updateBy,
                            CreateDate = utcNow
                        });
                    }
                }

                await _unitOfWork.CommitAsync();

                activityLogs.Add(new ActivityLog
                {
                    LogId = Guid.NewGuid(),
                    EntityId = job.JobId,
                    Entity = DVTEntities.JobFile,
                    Message = message,
                    CreateBy = job.UpdateBy,
                    CreateDate = DateTime.UtcNow
                });

                result.Success = true;
                result.Message = message;
                result.Data = job;
            }
            catch (Exception ex)
            {
                result.Success = true;
                result.Message = StardardMessages.JobFilesLoadedFailed;
                result.Exception = new Exception(ex.Message);
                activityLogs.Add(new ActivityLog
                {
                    LogId = Guid.NewGuid(),
                    EntityId = job.JobId,
                    Entity = DVTEntities.JobFile,
                    MessageType = LogMessageTypes.Error,
                    Message = Operations.LoadJobFiles + ", Exception Message: " + result.Message + ", " + ex.Message,
                    CreateBy = job.UpdateBy,
                    CreateDate = DateTime.UtcNow
                });
            }

            await _activityLogService.AddLogsAsync(activityLogs);
            return result;
        }

        public async ValueTask<Job> GetUserLatestActiveJobAsync(Guid userInfoId)
        {
            return await _unitOfWork.Jobs.GetUserLatestActiveJobAsync(userInfoId);
        }

        /// <summary>
        /// User Story 15931278: 4 - Job Service - Retrieve an active job
        /// </summary>
        /// <param name="userInfoId"></param>
        /// <returns></returns>
        public async ValueTask<OperationResult> GetActiveJobResultAsync(Guid userInfoId)
        {
            var activeJob = await _unitOfWork.Jobs.GetUserLatestActiveJobAsync(userInfoId);

            if (activeJob == null)
            {
                return new OperationResult
                {
                    Operation = Operations.GetActiveJob,
                    Success = false,
                    Message = StardardMessages.NoActiveJobFound
                };
            }
            else
            {
                var mapper = new Mapping();
                return new OperationResult
                {
                    Operation = Operations.GetActiveJob,
                    Success = true,
                    Message = StardardMessages.ActiveJobFound,
                    Data = mapper.JobToJobDto(activeJob)
                };
            }
        }

        public async ValueTask<OperationResult> GetJobStatusByIdAsync(Guid jobId)
        {
            var result = new OperationResult
            {
                Operation = Operations.GetJob,
            };

            var job = await GetJobWithJobFilesByIdAsync(jobId);
            //job.JobFiles = (List<JobFile>)await _jobFileService.GetJobFilesByJobIdAsync(jobId);

            result.Success = true;
            result.Data = job.Status;
            //result.Data = job;

            return result;
        }

        /// <summary>
        /// User Story 15927334: 5 - Job Service - Status management
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="status"></param>
        /// <param name="updateBy"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="KeyNotFoundException"></exception>
        public async ValueTask<OperationResult> UpdateJobStatusAsync(Guid jobId, string status, string updateBy)
        {
            var result = new OperationResult
            {
                Operation = Operations.UpdateJobStatus,
            };
            try
            {
                var job = await GetJobWithJobFilesByIdAsync(jobId);

                var constantJobStatus = GetJobStatus(status);

                if (string.IsNullOrEmpty(constantJobStatus))
                {
                    throw new ArgumentNullException(nameof(status), StardardMessages.IncorrectJobStatus);
                }

                job.Status = constantJobStatus;
                job.UpdateDate = DateTime.UtcNow;
                job.UpdateBy = updateBy;

                await _unitOfWork.CommitAsync();

                await _activityLogService.AddLogAsync(new ActivityLog
                {
                    LogId = Guid.NewGuid(),
                    EntityId = jobId,
                    Entity = DVTEntities.Job,
                    Message = Operations.UpdateJobStatus + " to " + status,
                    CreateBy = updateBy,
                    CreateDate = DateTime.UtcNow
                });

                result.Success = true;
                result.Message = StardardMessages.JobStatusUpdatedSuccessfully;
                result.Data = job;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = StardardMessages.JobStatusUpdatedFailed;
                result.Exception = new Exception(ex.Message);
                await _activityLogService.AddLogAsync(new ActivityLog
                {
                    LogId = Guid.NewGuid(),
                    EntityId = jobId,
                    Entity = DVTEntities.Job,
                    MessageType = LogMessageTypes.Error,
                    Message = Operations.UpdateJobStatus + ", Exception Message: " + result.Message + ", " + ex.Message,
                    CreateBy = updateBy,
                    CreateDate = DateTime.UtcNow
                });
            }
            return result;
        }

        /// <summary>
        /// User Story 15927333: 10.1 - Job Service - Delete a job
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="updateBy"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="KeyNotFoundException"></exception>
        public async ValueTask<OperationResult> DeleteJobAsync(Guid jobId, string updateBy, bool isRefresh = false)
        {
            var result = new OperationResult
            {
                Operation = isRefresh ? Operations.RefreshAndDeleteJob : Operations.DeleteJob,
            };

            try
            {
                var job = await GetJobWithJobFilesNoValidationByIdAsync(jobId);

                job.Deleted = true;
                job.UpdateDate = DateTime.UtcNow;
                job.UpdateBy = updateBy;
                await _unitOfWork.CommitAsync();

                await _activityLogService.AddLogAsync(new ActivityLog
                {
                    LogId = Guid.NewGuid(),
                    EntityId = jobId,
                    Entity = DVTEntities.Job,
                    Message = result.Operation,
                    CreateBy = updateBy,
                    CreateDate = DateTime.UtcNow
                });

                await _jobFileService.DeleteJobFilesAsync(jobId, updateBy, isRefresh);

                result.Success = true;
                result.Message = StardardMessages.JobDeletedSuccessfully;
                result.Data = job;
            }
            catch (Exception ex)
            {
                result.Exception = new Exception(ex.Message);
                result.Success = false;
                result.Message = StardardMessages.JobDeletedFailed;
                await _activityLogService.AddLogAsync(new ActivityLog
                {
                    LogId = Guid.NewGuid(),
                    EntityId = jobId,
                    Entity = DVTEntities.Job,
                    MessageType = LogMessageTypes.Error,
                    Message = result.Operation + ", Exception Message: " + result.Message + ", " + ex.Message,
                    CreateBy = updateBy,
                    CreateDate = DateTime.UtcNow
                });
            }

            return result;
        }

        public async ValueTask<Job> GetJobWithJobFilesByIdAsync(Guid jobId)
        {
            if (jobId == null || jobId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(jobId), StardardMessages.ObjectCannotBeNull);
            }

            var job = await _unitOfWork.Jobs.GetByIdAsync(jobId);

            if (job == null)
            {
                throw new KeyNotFoundException($"{StardardMessages.ItemNotFound} id: {jobId}");
            }

            var jobFiles = await _jobFileService.GetJobFilesByJobIdAsync(jobId);

            job.JobFiles = (List<JobFile>)jobFiles;

            return job;
        }

        public async ValueTask<Job> GetJobWithJobFilesNoValidationByIdAsync(Guid jobId)
        {
            if (jobId == null || jobId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(jobId), StardardMessages.ObjectCannotBeNull);
            }

            var job = await _unitOfWork.Jobs.GetByIdAsync(jobId);

            if (job == null)
            {
                throw new KeyNotFoundException($"{StardardMessages.ItemNotFound} id: {jobId}");
            }

            var jobFiles = await _jobFileService.GetJobFilesByJobIdNoValidationAsync(jobId);

            job.JobFiles = (List<JobFile>)jobFiles;

            return job;
        }

        public async ValueTask<Job> GetJobByIdNoTrackingAsync(Guid jobId)
        {
            if (jobId == null || jobId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(jobId), StardardMessages.ObjectCannotBeNull);
            }

            var job = await _unitOfWork.Jobs.GetByIdNoTrackingAsync(jobId);

            if (job == null)
            {
                throw new KeyNotFoundException($"{StardardMessages.ItemNotFound} id: {jobId}");
            }

            var jobFiles = await _jobFileService.GetJobFilesByJobIdNoValidationNoTrackingAsync(jobId);

            job.JobFiles = (List<JobFile>)jobFiles;

            return job;
        }

        /// <summary>
        /// User Story 15989155: 11 - Job Service - Refresh Process
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="updateBy"></param>
        /// <returns></returns>
        public async ValueTask<OperationResult> RefreshJobAsync(Guid jobId, string updateBy)
        {
            try
            {
                var job = await GetJobWithJobFilesByIdAsync(jobId);

                var deleteResult = await DeleteJobAsync(jobId, updateBy);

                if (deleteResult.Success)
                {
                    var result = await CreateJobAsync(new Job
                    {
                        DivisionId = job.DivisionId,
                        FeedNumber = job.FeedNumber,
                        UserInfoId = job.UserInfoId,
                        CreateBy = updateBy,
                        UpdateBy = updateBy
                    }, false);

                    result.Operation = Operations.RefreshJob;

                    return result;
                }
                else
                {
                    deleteResult.Operation = Operations.RefreshJob;

                    return deleteResult;
                }
            }
            catch (Exception ex)
            {
                await _activityLogService.AddLogAsync(new ActivityLog
                {
                    LogId = Guid.NewGuid(),
                    EntityId = jobId,
                    Entity = DVTEntities.Job,
                    MessageType = LogMessageTypes.Error,
                    Message = StardardMessages.RefreshJobFailed + ", Exception Message: " + ex.Message,
                    CreateBy = updateBy,
                    CreateDate = DateTime.UtcNow
                });

                return new OperationResult
                {
                    Operation = Operations.RefreshJob,
                    Success = false,
                    Message = StardardMessages.RefreshJobFailed,
                    Exception = new Exception(ex.Message)
                };
            }
        }

        /// <summary>
        /// User Story 16164153: DVT - Analysis Controller - Get Analysis statistics for job
        /// Task 19298918: DVT - Analysis Controller - Get Analysis statistics for job - Service
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public async ValueTask<OperationResult> GetJobValidationStatsByJobIdAsync(Guid jobId, string userEmail)
        {
            var jobFiles = await _unitOfWork.JobFiles.GetJobFilesByJobIdAsync(jobId);
            if (jobFiles == null)
            {
                throw new KeyNotFoundException($"{StardardMessages.ItemNotFound} for Job Id: {jobId}");
            }

            var result = new OperationResult()
            {
                Operation = Operations.GetJobStatistics,
                Success = true,
            };

            try
            {
                var fileType = "";
                var fileName = "";
                var validationStats = "";
                var statsResult = new List<FileValidationStatsResult>();
                foreach (var jobFile in jobFiles)
                {
                    fileType = jobFile.FileType;
                    fileName = jobFile.FileName;

                    var newFileValidationStatsResultDto = new FileValidationStatsResult
                    {
                        FileName = fileName,
                        Date = jobFile.UpdateDate,
                        FileType = jobFile.FileType,
                        TableName = jobFile.TableName,
                    };

                    validationStats = jobFile.ValidationStats;

                    if (fileType == FileTypes.Vir)
                    {
                        newFileValidationStatsResultDto.Stats = string.IsNullOrWhiteSpace(validationStats) ? null : JsonConvert.DeserializeObject<FileCalculateStatistics_Vir>(validationStats);
                    }
                    else if (fileType == FileTypes.Vir)
                    {

                    }
                    else if (fileType == FileTypes.Vir)
                    {
                    }
                    statsResult.Add(newFileValidationStatsResultDto);
                }

                result.Data = statsResult;
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
                    Message = Operations.GetJobStatistics + ", Exception Message: " + result.Message + ", " + ex.Message,
                    CreateBy = userEmail,
                    CreateDate = DateTime.UtcNow
                });
            }
            return result;
        }
        
        public async ValueTask<OperationResult> CleanupJobWorkingDirectory(Guid jobId, string updateBy)
        {
            var result = new OperationResult
            {
                Operation = Operations.CleanupJobWorkingDirectory,
            };

            try
            {
                await _storageService.CleanupJobWorkingDirectoryAsync(jobId);
                result.Success = true;
                result.Message = StardardMessages.CleanupJobFilesSuccessfully;
            }
            catch (Exception ex)
            {
                result.Success = true;
                result.Message = StardardMessages.CleanupJobFilesFailed;
                result.Exception = new Exception(ex.Message);
                await _activityLogService.AddLogAsync(new ActivityLog
                {
                    LogId = Guid.NewGuid(),
                    EntityId = jobId,
                    Entity = DVTEntities.JobFile,
                    MessageType = LogMessageTypes.Error,
                    Message = Operations.UpdateJob + ", Exception Message: " + result.Message + ", " + ex.Message,
                    CreateBy = updateBy,
                    CreateDate = DateTime.UtcNow
                });
            }

            return result;
        }
        
        /// <summary>
        /// User Story 16176113: 8 - Job Service - Accept Validation Errors
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="jobFileId"></param>
        /// <param name="updateBy"></param>
        /// <returns></returns>
        public async ValueTask<OperationResult> AcceptValidationResultAsync(Guid jobId, Guid jobFileId, string updateBy)
        {
            var result = new OperationResult
            {
                Operation = Operations.AcceptValidationResult,
            };

            try
            {
                var jobFile = await _jobFileService.GetJobFileByIdNoValidationAsync(jobFileId);

                if (jobFile == null)
                {
                    throw new KeyNotFoundException($"{StardardMessages.ItemNotFound} for Job File Id: {jobFileId}");
                }

                var currentJobFileStatus = jobFile.Status;

                if (currentJobFileStatus != WellKnownFileStatuses.Validated && currentJobFileStatus != WellKnownFileStatuses.Warning && currentJobFileStatus != WellKnownFileStatuses.Errors)
                {
                    result.Success = false;
                    result.Message = StardardMessages.AcceptValidationNotAllowed;
                    return result;
                }

                var updateJobFileResult = await _jobFileService.UpdateJobFileStatusAsync(jobFileId, WellKnownFileStatuses.Accepted, updateBy);

                if (updateJobFileResult.Success)
                {
                    var archiveResult = await ArchiveResultAndCompleteJobAsync(jobId, new List<JobFile> { jobFile }, new List<Guid> { jobFileId }, updateBy);

                    if (archiveResult.Success)
                    {
                        result.Success = true;
                        //User Story 23182006: DVT - Duplicated job creation issue fix
                        result.Data = archiveResult.Data;
                        result.Message = StardardMessages.AcceptValidationSuccessfully;
                    }
                    else
                    {
                        result.Success = false;
                        result.Message = StardardMessages.CompleteJobAndArchiveZipFilesFailed;
                    }
                }
                else
                {
                    result.Success = false;
                    result.Message = StardardMessages.JobFileStatusUpdatedFailed;
                }

                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = StardardMessages.AcceptValidationFailed;
                result.Exception = new Exception(ex.Message);

                AddLog(jobId, Operations.AcceptValidationResult + ", Exception Message: " + result.Message + ", " + ex.Message, updateBy, true, LogMessageTypes.Error).Wait();
            }

            return result;
        }

        public async ValueTask<JobStatusUpdate> GetJobAndFileStatusByJobIdAsync(Guid jobId)
        {
            var job = await GetJobWithJobFilesNoValidationByIdAsync(jobId);
            var jobStatusUpdate = new JobStatusUpdate(jobId, job.Status);

            if (job != null && job.JobFiles != null && job.JobFiles.Any())
            {
                job.JobFiles.ForEach(f =>
                {
                    jobStatusUpdate.AddFileStatus(f.JobFileId, f.Status);
                });
            }

            return jobStatusUpdate;
        }

        private async ValueTask<UserInfo> GetUserInfoAndCheckDirectory(Guid userInfoId)
        {
            var userInfo = await _userInfoService.GetByIdAsync(userInfoId);

            if (string.IsNullOrEmpty(userInfo.LoadFolder))
            {
                throw new Exception(StardardMessages.UserHasNotSetLoadDirectory);
            }

            if (string.IsNullOrEmpty(userInfo.LogFolder))
            {
                throw new Exception(StardardMessages.UserHasNotSetLogDirectory);
            }

            if (string.IsNullOrEmpty(userInfo.ProductionFolder))
            {
                throw new Exception(StardardMessages.UserHasNotSetProductionDirectory);
            }

            return userInfo;
        }

        private async ValueTask<List<JobFile>> GetJobFilesAsync(string userEmailAddress, string loadFolder, Guid jobId, string divisionAbbrev, int feedNumber, List<ActivityLog> activityLogs)
        {
            var message = "";
            var utcNow = DateTime.UtcNow;

            var files = await _storageService.GetFileInfoInDirectoryAsync(loadFolder);

            if (files == null || files.Count == 0)
            {
                message = string.Format(StardardMessages.NoFilesFoundInDirectory, loadFolder);

                activityLogs.Add(new ActivityLog
                {
                    LogId = Guid.NewGuid(),
                    EntityId = jobId,
                    Entity = DVTEntities.JobFile,
                    Message = message,
                    CreateBy = userEmailAddress,
                    CreateDate = utcNow
                });

                throw new Exception(message);
            }

            var jobFiles = new List<JobFile>();

            var divisionAbbrevLoer = divisionAbbrev.ToLower();

            var fileName = "";
            var fileNameLower = "";
            FileTemplate fileTemplateItem;
            JobFile jobFile;
            Guid jobFileId;
            foreach (var file in files)
            {
                jobFileId = Guid.NewGuid();
                fileName = file.FileName;
                fileNameLower = fileName.ToLower();
                fileTemplateItem = GetFileTableTypeRelatedInfo(fileName, divisionAbbrevLoer, feedNumber);

                if (fileTemplateItem == null)
                {
                    message += (string.Format(StardardMessages.IncorrectFileFormat, fileName));
                    continue;
                }

                jobFile = new JobFile
                {
                    JobId = jobId,
                    JobFileId = jobFileId,
                    FileName = fileName,
                    FilePath = file.FilePath,
                    FileType = "",
                    TableName = "",
                    DependsOnFileType = "",
                    RecordCount = file.RecordCount,
                    SortOrder = 0,
                    FileCreationTimestamp = file.FileCreationTimestamp,
                    FileLastModifiedTimestamp = file.FileLastModifiedTimestamp,
                    Status = WellKnownFileStatuses.New,
                    ValidationMessages = null,
                    UpdateBy = userEmailAddress,
                    UpdateDate = utcNow,
                };

                AnalyzeFile(jobFile, fileTemplateItem, fileNameLower, divisionAbbrev.ToLower(), feedNumber);

                jobFiles.Add(jobFile);

                activityLogs.Add(new ActivityLog
                {
                    LogId = Guid.NewGuid(),
                    EntityId = jobFileId,
                    Entity = DVTEntities.JobFile,
                    Message = Operations.AddJobFile + $": {file.FilePath}",
                    CreateBy = userEmailAddress,
                    CreateDate = utcNow
                });
            }

            return jobFiles;
        }

        private void AnalyzeFile(JobFile file, FileTemplate fileTemplateItem, string fileNameLower, string divisionAbbrevLower, int feedNumber)
        {
            file.TableName = fileTemplateItem?.Table ?? "";
            file.FileType = fileTemplateItem?.FileType ?? "";
            file.SortOrder = fileTemplateItem?.SortOrder ?? 0;
            file.DependsOnFileType = fileTemplateItem?.DependsOnFileTypes ?? "";
        }

        private FileTemplate GetFileTableTypeRelatedInfo(string fileName, string divisionAbbrevLower, int feedNumber)
        {
            return FileTemplateList.FirstOrDefault(val => fileName.Equals(val.FileNameFormat.Replace(FileNameFormats.DivAbbrev, divisionAbbrevLower).Replace(FileNameFormats.FeedNumber, feedNumber.ToString()), StringComparison.OrdinalIgnoreCase));
        }

        private string GetJobStatus(string status)
        {
            if (status.Equals(WellKnownJobStatuses.New, StringComparison.OrdinalIgnoreCase))
            {
                return WellKnownJobStatuses.New;
            }
            else if (status.Equals(WellKnownJobStatuses.Uploaded, StringComparison.OrdinalIgnoreCase))
            {
                return WellKnownJobStatuses.Uploaded;
            }
            else if (status.Equals(WellKnownJobStatuses.Validated, StringComparison.OrdinalIgnoreCase))
            {
                return WellKnownJobStatuses.Validated;
            }
            else if (status.Equals(WellKnownJobStatuses.Failed, StringComparison.OrdinalIgnoreCase))
            {
                return WellKnownJobStatuses.Failed;
            }
            else if (status.Equals(WellKnownJobStatuses.InProgress, StringComparison.OrdinalIgnoreCase))
            {
                return WellKnownJobStatuses.InProgress;
            }
            else if (status.Equals(WellKnownJobStatuses.Completed, StringComparison.OrdinalIgnoreCase))
            {
                return WellKnownJobStatuses.Completed;
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// User Story 16255470: 10 - Job Service - create output files
        /// User Story 20178973: DVT – Complete Button After Accepting Files
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="jobFiles"></param>
        /// <param name="selectedFileIds"></param>
        /// <param name="updateBy"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        private async ValueTask<OperationResult> ArchiveResultAndCompleteJobAsync(Guid jobId, IEnumerable<JobFile> jobFiles, List<Guid> selectedFileIds, string updateBy)
        {
            var result = new OperationResult
            {
                Operation = Operations.CompleteJobAndArchiveZipFiles,
            };

            try
            {
                var job = await _unitOfWork.Jobs.GetByIdAsync(jobId);

                var selectedAcceptedFiles = jobFiles.Where(f => selectedFileIds.Contains(f.JobFileId)).ToList();

                if (selectedAcceptedFiles == null || selectedAcceptedFiles.Count == 0)
                {
                    result.Success = false;
                    result.Message = StardardMessages.NoAcceptedFilesSelectedForComplete;
                    AddLog(jobId, StardardMessages.NoAcceptedFilesSelectedForComplete + ", jobId: " + jobId, updateBy, true, LogMessageTypes.Error).Wait();
                    return result;
                }

                var userInfo = await _userInfoService.GetByIdAsync(job.UserInfoId);

                if (userInfo == null)
                {
                    throw new KeyNotFoundException($"{StardardMessages.ItemNotFound} for User Info Id: {job.UserInfoId}");
                }

                var outputFolder = userInfo.ProductionFolder;
                if (string.IsNullOrEmpty(userInfo.ProductionFolder))
                {
                    throw new Exception(StardardMessages.ProductionDirectoryIsEmpty);
                }

                //This problem may occur with multiple files. The first file is from the end of month accept and has an archive path A. The last file is accepted at the beginning of next month and may have a new path B. If there is an archive path, it will not be generated to avoid being placed in multiple folders due to different months.
                if (!string.IsNullOrWhiteSpace(job.ArchiveFilePath))
                {
                    outputFolder = job.ArchiveFilePath;
                }
                else
                {
                    var lastMonth = DateTime.UtcNow.AddMonths(-1);
                    var lastMonthFolderName = $"{lastMonth.ToString("MMMM")}{lastMonth.Year}";
                    outputFolder = outputFolder + $"/{lastMonthFolderName}";
                }

                //the zip already exists after the validation, so move them to outputFolder.
                await _storageService.ArchiveZipLogFilesAsync(jobId, selectedAcceptedFiles, outputFolder, updateBy);

                jobFiles = await _jobFileService.GetJobFilesByJobIdNoValidationNoTrackingAsync(jobId);

                //Take the json files from the working directory, zip them and copy the file to the archives folder.
                await ArchiveJobValidationResultsAsync(jobId, jobFiles);

                //If all the JobFiles are accepted, then set the status of the job to Completed
                if (jobFiles.All(x => x.Status.Equals(WellKnownFileStatuses.Accepted)))
                {
                    job.Status = WellKnownJobStatuses.Completed;

                    //Copy the complete zip file to the supply chain folder for downstream system to pick up
                    await _storageService.CopyOutputFilesToSupplyChainFolderAsync(jobId, selectedAcceptedFiles);

                    //User Story 16171132: 7 - Job Controller - Close Job --- Job Service - Cleanup Job Files                        
                    await CleanupJobWorkingDirectory(jobId, updateBy);
                }              

                //update Archive file path
                job.ArchiveFilePath = outputFolder;
                job.UpdateDate = DateTime.UtcNow;
                job.UpdateBy = updateBy;
                await UpdateJobAsync(job);

                var mapper = new Mapping();
                result.Success = true;
                result.Data = mapper.JobToJobDto(job);
                result.Message = StardardMessages.CompleteJobAndArchiveZipFilesSuccessfully;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = StardardMessages.CompleteJobAndArchiveZipFilesFailed;
                result.Exception = new Exception(ex.Message);
                AddLog(jobId, StardardMessages.CompleteJobAndArchiveZipFilesFailed + ", Exception Message: " + result.Message + ", " + ex.Message, updateBy, true, LogMessageTypes.Error).Wait();
            }

            return result;
        }

        /// <summary>
        /// Takes the validation JSON files in the working directory, compresses them to a zip folder and copies them to the Archive folder in the main-share
        /// </summary>
        /// <param name="jobId">GUID of the job</param>        
        private async Task ArchiveJobValidationResultsAsync(Guid jobId, IEnumerable<JobFile> jobFiles)
        {
            try
            {
                //Get the list of files that need to be zipped
                var validationJobFileNames = jobFiles.Select(x => x.GetValidationMessageFileName()).ToList();

                //Zip up the files
                await _storageService.CompressFilesToZipInWorkingFolderAsync(jobId, "validation_results.zip", validationJobFileNames);

                //Copy the zip file to the archive directory
                await _storageService.CopyFileFromWorkingFolderToArchiveFolderAsync(jobId, "validation_results.zip");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error Archiving validation results {jobId}", ex);
            }
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

    }
}