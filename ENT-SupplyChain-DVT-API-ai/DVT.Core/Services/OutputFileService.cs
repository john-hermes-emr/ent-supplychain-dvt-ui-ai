using DVT.Core.Models;
using Newtonsoft.Json;
using static DVT.Core.Constants;

namespace DVT.Core.Services
{
    public class OutputFileService : IOutputFileService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStorageService _storageService;
        private readonly IActivityLogService _activityLogService;
        private readonly IUserInfoService _userInfoService;

        public OutputFileService(IUnitOfWork unitOfWork, IStorageService storageService, IActivityLogService activityLogService, IUserInfoService userInfoService)
        {
            _unitOfWork = unitOfWork;
            _storageService = storageService;
            _activityLogService = activityLogService;
            _userInfoService = userInfoService;
        }

        /// <summary>
        /// May No need it.
        /// User Story 13619011: 1 - Output File Service - Create Output File
        /// User Story 16255470: 10 - Job Service - create output files
        /// </summary>
        /// <param name="job"></param>
        /// <param name="jobFiles"></param>
        /// <param name="outputFolder"></param>
        /// <param name="updateBy"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public async ValueTask<OperationResult> CreateOutputFilesAsync(Job job, IEnumerable<JobFile> jobFiles, string outputFolder, string updateBy)
        {
            var result = new OperationResult
            {
                Operation = Operations.CreateOutputFiles,
            };

            try
            {
                var jobId = job.JobId;
                var jobStatus = job.Status;

                if (jobStatus != WellKnownJobStatuses.Completed)
                {
                    result.Success = false;
                    result.Message = StardardMessages.JobIsNotCompleted;
                    return result;
                }

                //The month of year shall always be one month less than the curren't month.
               

                result.Success = true;
                result.Message = StardardMessages.CreateOutputFileSuccessful;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = StardardMessages.CreateOutputFileFailed;
                result.Exception = new Exception(ex.Message);
                await _activityLogService.AddLogAsync(new ActivityLog
                {
                    LogId = Guid.NewGuid(),
                    EntityId = job.JobId,
                    Entity = DVTEntities.Job,
                    MessageType = LogMessageTypes.Error,
                    Message = StardardMessages.CreateOutputFileFailed + ", Exception Message: " + result.Message + ", " + ex.Message,
                    CreateBy = updateBy,
                    CreateDate = DateTime.UtcNow
                });
            }

            return result;
        }
    }
}
