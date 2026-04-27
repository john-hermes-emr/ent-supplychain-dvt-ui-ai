using DVT.Api.Contracts;
using DVT.Api.Contracts.File;
using DVT.Api.Extensions;
using DVT.Core.Models;
using DVT.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using static DVT.Core.Constants;

namespace DVT.Api.Controllers
{
    [Authorize]
    [EnableCors(ApiRoutes.SecurityDecorations.CorsAllowAll)]
    public class AnalysisController : ControllerBase
    {
        private readonly IJobFileService _jobFileService;
        private readonly IJobService _jobService;

        public AnalysisController(IJobFileService jobFileService, IJobService jobService)
        {
            _jobFileService = jobFileService;
            _jobService = jobService;
        }

        [HttpPost(ApiRoutes.Analysis.GetAnalysisErrorsByJobIdAndJobFileId)]
        public async Task<ActionResult<FileValidationErrorResult>> GetAnalysisErrorsByJobIdAndJobFileIdAsync([FromBody] GetFileValidationResultRequest getFileValidationResultRequest)
        {
            if (getFileValidationResultRequest == null)
            {
                return NotFound(StardardMessages.ObjectCannotBeNull);
            }

            if (getFileValidationResultRequest.JobId == null || getFileValidationResultRequest.JobId == Guid.Empty)
            {
                return NotFound(StardardMessages.JobIdCannotBeNull);
            }

            if (getFileValidationResultRequest.JobFileId == null || getFileValidationResultRequest.JobFileId == Guid.Empty)
            {
                return NotFound(StardardMessages.JobFileIdCannotBeNull);
            }

            var emailAddress = HttpContext.GetUserEmailFromHttpContext();

            var jobId = getFileValidationResultRequest.JobId;
            var jobFileId = getFileValidationResultRequest.JobFileId;

            var errors = await _jobFileService.GetJobFileValidationMessageByJobIdAndJobFileIdAsync(jobId, jobFileId, emailAddress);
            return Ok(errors);
        }      

        [HttpPost(ApiRoutes.Analysis.GenerateAnalysisErrorReportByJobIdAndJobFileId)]
        public async Task<ActionResult<FileValidationErrorResult>> GenerateAnalysisErrorReportByJobIdAndJobFileIdAsync([FromBody] GetFileValidationResultRequest getFileValidationResultRequest)
        {
            if (getFileValidationResultRequest == null)
            {
                return NotFound(StardardMessages.ObjectCannotBeNull);
            }

            if (getFileValidationResultRequest.JobId == null || getFileValidationResultRequest.JobId == Guid.Empty)
            {
                return NotFound(StardardMessages.JobIdCannotBeNull);
            }

            if (getFileValidationResultRequest.JobFileId == null || getFileValidationResultRequest.JobFileId == Guid.Empty)
            {
                return NotFound(StardardMessages.JobFileIdCannotBeNull);
            }

            var emailAddress = HttpContext.GetUserEmailFromHttpContext();

            var jobId = getFileValidationResultRequest.JobId;
            var jobFileId = getFileValidationResultRequest.JobFileId;

            var jobFile = await _jobFileService.GetJobFileByIdAsync(jobFileId);

            if (jobFile == null)
            {
                return NotFound(StardardMessages.NoFileFound + jobFileId);
            }

            var report = await _jobFileService.GenerateJobFileErrorReportByJobIdAndJobFileIdAsync(jobId, jobFileId, emailAddress);

            return File(report, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", string.Format(ReportNames.ErrorReportName, jobFile.TableName, DateTime.Now.ToString("yyyy-MM-dd")));
        }

        [HttpGet(ApiRoutes.Analysis.GetAnalysisStatsByJobId)]
        public async Task<ActionResult<FileValidationErrorResult>> GetAnalysisStatsByJobIdAsync(Guid id)
        {
            if (id == null || id == Guid.Empty)
            {
                return NotFound(StardardMessages.ObjectCannotBeNull);
            }

            var emailAddress = HttpContext.GetUserEmailFromHttpContext();

            var stats = await _jobService.GetJobValidationStatsByJobIdAsync(id, emailAddress);
            return Ok(stats);
        }

        [HttpPost(ApiRoutes.Analysis.GetAnalysisStatsByJobIdAndJobFileId)]
        public async Task<ActionResult<FileValidationErrorResult>> GetAnalysisStatsByJobIdAndJobFileIdAsync([FromBody] GetFileValidationResultRequest getFileValidationResultRequest)
        {
            if (getFileValidationResultRequest == null)
            {
                return NotFound(StardardMessages.ObjectCannotBeNull);
            }

            if (getFileValidationResultRequest.JobId == null || getFileValidationResultRequest.JobId == Guid.Empty)
            {
                return NotFound(StardardMessages.JobIdCannotBeNull);
            }

            if (getFileValidationResultRequest.JobFileId == null || getFileValidationResultRequest.JobFileId == Guid.Empty)
            {
                return NotFound(StardardMessages.JobFileIdCannotBeNull);
            }

            var emailAddress = HttpContext.GetUserEmailFromHttpContext();

            var jobId = getFileValidationResultRequest.JobId;
            var jobFileId = getFileValidationResultRequest.JobFileId;

            var stats = await _jobFileService.GetJobValidationStatsByJobIdAndJobFileIdAsync(jobId, jobFileId, emailAddress);
            return Ok(stats);
        }

        [HttpPost(ApiRoutes.Analysis.GenerateAnalysisStatsReportByJobIdAndJobFileId)]
        public async Task<ActionResult<FileValidationErrorResult>> GenerateAnalysisStatsReportByJobIdAndJobFileIdAsync([FromBody] GetFileValidationResultRequest getFileValidationResultRequest)
        {
            if (getFileValidationResultRequest == null)
            {
                return NotFound(StardardMessages.ObjectCannotBeNull);
            }

            if (getFileValidationResultRequest.JobId == null || getFileValidationResultRequest.JobId == Guid.Empty)
            {
                return NotFound(StardardMessages.JobIdCannotBeNull);
            }

            if (getFileValidationResultRequest.JobFileId == null || getFileValidationResultRequest.JobFileId == Guid.Empty)
            {
                return NotFound(StardardMessages.JobFileIdCannotBeNull);
            }

            var emailAddress = HttpContext.GetUserEmailFromHttpContext();

            var jobId = getFileValidationResultRequest.JobId;
            var jobFileId = getFileValidationResultRequest.JobFileId;

            var jobFile = await _jobFileService.GetJobFileByIdAsync(jobFileId);

            if (jobFile == null)
            {
                return NotFound(StardardMessages.NoFileFound + jobFileId);
            }

            var report = await _jobFileService.GenerateJobFileStatsReportByJobIdAndJobFileIdAsync(jobId, jobFileId, emailAddress);

            return File(report, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", string.Format(ReportNames.StatisticsReportName, jobFile.TableName, DateTime.Now.ToString("yyyy-MM-dd")));
        }
    }
}
