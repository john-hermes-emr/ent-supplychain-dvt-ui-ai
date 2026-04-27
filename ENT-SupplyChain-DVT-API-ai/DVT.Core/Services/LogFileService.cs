using DVT.Core.Models;
using Newtonsoft.Json;
using System.Text;
using static DVT.Core.Constants;

namespace DVT.Core.Services
{
    public class LogFileService : ILogFileService
    {
        private readonly IStorageService _storageService;
        private readonly IActivityLogService _activityLogService;
        private readonly IUserInfoService _userInfoService;
        private readonly IJobFileService _jobFileService;


        public LogFileService(IStorageService storageService, IActivityLogService activityLogService, IUserInfoService userInfoService, IJobFileService jobFileService)
        {
            _storageService = storageService;
            _activityLogService = activityLogService;
            _userInfoService = userInfoService;
            _jobFileService = jobFileService;
        }

        /// <summary>
        ///User Story 19441377: Validation output enhancement --- log file creation
        /// </summary>
        /// <param name="job"></param>
        /// <param name="jobFiles"></param>
        /// <param name="outputFolder"></param>
        /// <param name="updateBy"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public async ValueTask<OperationResult> CreateLogFilesAsync(Job job, List<JobFile> validatedFiles, string updateBy)
        {
            var result = new OperationResult
            {
                Operation = Operations.CreateLogFiles,
            };

            var userInfo = await _userInfoService.GetByIdAsync(job.UserInfoId);

            if (userInfo == null)
            {
                throw new KeyNotFoundException($"{StardardMessages.ItemNotFound} for User Info Id: {job.UserInfoId}");
            }

            var logFolder = userInfo.LogFolder;
            if (string.IsNullOrEmpty(logFolder))
            {
                throw new Exception(StardardMessages.LogDirectoryIsEmpty);
            }

            var errorSB = new StringBuilder();
            var fileName = "";
            var recordCount = 0;
            var validationMessage = "";

            var criticalRowNums = new List<int>();
            var errorsRowNums = new List<int>();
            var warningRowNums = new List<int>();

            foreach (var file in validatedFiles)
            {
                criticalRowNums.Clear();
                errorsRowNums.Clear();
                warningRowNums.Clear();
                recordCount = 0;
                try
                {
                    if (file == null)
                    {
                        continue;
                    }

                    if (!WellKnownFileValidatedStatusList.Contains(file.Status))
                    {
                        //only create log files for validated files.
                        continue;
                    }

                    fileName = file.FileName;
                    recordCount = file.RecordCount;

                    //Get the validation messages from the file stored in the job working directory
                    validationMessage = await _jobFileService.GetJobFileValidationFileContentsByJobFileAsync(file);

                    if (!string.IsNullOrEmpty(validationMessage))
                    {
                        var rowValidationResults = JsonConvert.DeserializeObject<List<FileRowValidationResult>>(validationMessage);
                        if (rowValidationResults != null)
                        {
                            rowValidationResults.ForEach(v =>
                            {
                                criticalRowNums.AddRange(v.ValidationResult.Errors.Where(r => string.Equals(r.ErrorCode, WellKnownFileStatuses.Critical, StringComparison.OrdinalIgnoreCase)).Select(r => v.RowNumber).Distinct());

                                errorsRowNums.AddRange(v.ValidationResult.Errors.Where(r => string.Equals(r.ErrorCode, WellKnownFileStatuses.Errors, StringComparison.OrdinalIgnoreCase)).Select(r => v.RowNumber).Distinct());

                                warningRowNums.AddRange(v.ValidationResult.Errors.Where(r => string.Equals(r.ErrorCode, WellKnownFileStatuses.Warning, StringComparison.OrdinalIgnoreCase)).Select(r => v.RowNumber).Distinct());
                            });

                            await _storageService.CreateLogFilesAsync(job.JobId, file.JobFileId, fileName, recordCount, logFolder, criticalRowNums, errorsRowNums, warningRowNums, file.UpdateDate, updateBy);
                        }
                    }
                    else
                    {
                        //no validation messages, create empty log file.
                        await _storageService.CreateLogFilesAsync(job.JobId, file.JobFileId, fileName, recordCount, logFolder, criticalRowNums, errorsRowNums, warningRowNums, file.UpdateDate, updateBy);
                    }

                    result.Success = true;
                }
                catch (Exception ex)
                {
                    AddLog(file.JobFileId, ex.Message, updateBy).Wait();

                    errorSB.Append(ex.Message);
                }
            }

            if (errorSB.Length != 0)
            {
                result.Success = false;

                result.Message = errorSB.ToString();
            }

            return result;
        }

        private async Task AddLog(Guid entityId, string message, string updateBy, bool isJob = false, string messageType = "")
        {
            await _activityLogService.AddLogAsync(new ActivityLog
            {
                LogId = Guid.NewGuid(),
                EntityId = entityId,
                Entity = isJob ? DVTEntities.Job : DVTEntities.JobLogFile,
                MessageType = string.IsNullOrEmpty(messageType) ? LogMessageTypes.Info : messageType,
                Message = Operations.CreateLogFiles + ", " + message,
                CreateBy = updateBy,
                CreateDate = DateTime.UtcNow
            });
        }
    }
}
