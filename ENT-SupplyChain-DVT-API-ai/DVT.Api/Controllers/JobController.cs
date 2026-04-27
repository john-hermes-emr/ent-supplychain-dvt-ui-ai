using DVT.Api.Contracts;
using DVT.Api.Contracts.Job;
using DVT.Api.Extensions;
using DVT.Api.Mapping;
using DVT.Core.Models;
using DVT.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using static DVT.Core.Constants;

namespace DVT.Api.Controllers
{
    [Authorize]
    [EnableCors(ApiRoutes.SecurityDecorations.CorsAllowAll)]
    public class JobController : ControllerBase
    {
        private readonly IJobService _jobService;
        public JobController(IJobService jobService)
        {
            _jobService = jobService;
        }

        /// <summary>
        /// User Story 16171127: 2 - Job Controller - Create Job
        /// </summary>
        /// <param name="jobCreateRequest"></param>
        /// <returns></returns>
        [HttpPost(ApiRoutes.Jobs.CreateJob)]
        public async Task<ActionResult<OperationResult>> CreateJobAsync([FromBody] JobCreateRequest jobCreateRequest)
        {
            if (jobCreateRequest == null)
            {
                return NotFound(StardardMessages.ObjectCannotBeNull);
            }

            if (jobCreateRequest.UserInfoId == null)
            {
                return NotFound(StardardMessages.UserInfoIdCannotBeNull);
            }

            if (jobCreateRequest.DivisionId == null || jobCreateRequest.DivisionId == Guid.Empty)
            {
                return NotFound(StardardMessages.DivisionIdCannotBeNull);
            }

            if (jobCreateRequest.FeedNumber == null)
            {
                return NotFound(StardardMessages.FeedNumberCannotBeNull);
            }

            var mapper = new Mapper();
            var job = mapper.JobCreateRequestToJob(jobCreateRequest);

            var result = await _jobService.CreateJobAsync(job, jobCreateRequest.ForceCreate);

            return Ok(result);
        }

        /// <summary>
        /// Initialize job directory and copy files to the job directory
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        [HttpPost(ApiRoutes.Jobs.LoadExtractFiles)]
        public async Task<ActionResult<OperationResult>> LoadExtractFilesAsync([FromBody] InitJobFileRequest initJobFileRequest)
        {
            if (initJobFileRequest == null)
            {
                return NotFound(StardardMessages.ObjectCannotBeNull);
            }

            if (initJobFileRequest.JobId == null || initJobFileRequest.JobId == Guid.Empty)
            {
                return NotFound(StardardMessages.JobIdCannotBeNull);
            }

            var emailAddress = HttpContext.GetUserEmailFromHttpContext();

            var result = await _jobService.LoadExtractFilesAsync(initJobFileRequest.JobId, emailAddress);
            return Ok(result);
        }

        [HttpDelete(ApiRoutes.Jobs.DeleteJob)]
        public async Task<ActionResult<OperationResult>> DeleteJobAsync([FromRoute] Guid id)
        {
            if (id == null || id == Guid.Empty)
            {
                return NotFound(StardardMessages.ObjectCannotBeNull);
            }

            var emailAddress = HttpContext.GetUserEmailFromHttpContext();

            var result = await _jobService.DeleteJobAsync(id, emailAddress);
            return Ok(result);
        }

        /// <summary>
        /// User Story 16171126: 1 - Job Controller - Get Active Job
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet(ApiRoutes.Jobs.GetUserActiveJob)]
        public async Task<ActionResult<OperationResult>> GetUserActiveJobAsync([FromRoute] Guid id)
        {
            if (id == null || id == Guid.Empty)
            {
                return NotFound(StardardMessages.ObjectCannotBeNull);
            }

            var job = await _jobService.GetActiveJobResultAsync(id);

            return Ok(job);
        }

        /// <summary>
        /// User Story 16171128: 3 - Job Controller - Get Job Status
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet(ApiRoutes.Jobs.GetJobStatus)]
        public async Task<ActionResult<OperationResult>> GetJobStatusAsync([FromRoute] Guid id)
        {
            if (id == null || id == Guid.Empty)
            {
                return NotFound(StardardMessages.ObjectCannotBeNull);
            }

            var fileStatus = await _jobService.GetJobAndFileStatusByJobIdAsync(id);

            return Ok(fileStatus);
        }
    }
}
