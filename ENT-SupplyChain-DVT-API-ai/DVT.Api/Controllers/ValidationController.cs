using DVT.Api.Contracts;
using DVT.Api.Contracts.File;
using DVT.Api.Contracts.Job;
using DVT.Api.Extensions;
using DVT.Core.Models;
using DVT.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using static DVT.Core.Constants;

namespace DVT.Api.Controllers
{
    [Authorize]
    [EnableCors(ApiRoutes.SecurityDecorations.CorsAllowAll)]
    public class ValidationController : ControllerBase
    {
        private readonly IValidationService _validationService;
        private readonly IJobService _jobService;

        public ValidationController(IValidationService validationService, IJobService jobService)
        {
            _validationService = validationService;
            _jobService = jobService;
        }
        /// <summary>
        /// User Story 16171129: 4 - Job Controller - Validate Files
        /// </summary>
        /// <param name="jobValidationRequest"></param>
        /// <returns></returns>
        [HttpPost(ApiRoutes.Validations.ValidateFiles)]
        public async Task<ActionResult<OperationResult>> ValidateFilesAsync([FromBody] JobValidationRequest jobValidationRequest)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            if (jobValidationRequest == null)
            {
                return NotFound(StardardMessages.ObjectCannotBeNull);
            }

            if (jobValidationRequest.JobId == null || jobValidationRequest.JobId == Guid.Empty)
            {
                return NotFound(StardardMessages.JobIdCannotBeNull);
            }

            if (jobValidationRequest.SelectedFileIds == null || !jobValidationRequest.SelectedFileIds.Any())
            {
                return NotFound(StardardMessages.JobFileIdCannotBeNull);
            }

            var emailAddress = HttpContext.GetUserEmailFromHttpContext();

            var result = await _validationService.ValidateFilesAsync(jobValidationRequest.JobId, jobValidationRequest.SelectedFileIds, emailAddress);
            
            stopwatch.Stop();
            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            result.Message = $"Validation completed in {elapsedMilliseconds} ms.";

            return Ok(result);
        }

        /// <summary>
        /// User Story 16171130: 5 - Job Controller - Accept Validation Result
        /// User Story 16171131: 6 - Job Controller - Generate Output
        /// </summary>
        /// <param name="acceptValidationFileRequest"></param>
        /// <returns></returns>
        [HttpPost(ApiRoutes.Validations.AcceptValidation)]
        public async Task<ActionResult<OperationResult>> AcceptValidationAsync([FromBody] AcceptValidationRequest acceptValidationFileRequest)
        {
            if (acceptValidationFileRequest == null)
            {
                return NotFound(StardardMessages.ObjectCannotBeNull);
            }

            if (acceptValidationFileRequest.JobId == null || acceptValidationFileRequest.JobId == Guid.Empty)
            {
                return NotFound(StardardMessages.JobIdCannotBeNull);
            }

            if (acceptValidationFileRequest.JobFileId == null || acceptValidationFileRequest.JobFileId == Guid.Empty)
            {
                return NotFound(StardardMessages.JobFileIdCannotBeNull);
            }

            if (string.IsNullOrWhiteSpace(acceptValidationFileRequest.UpdateBy))
            {
                return NotFound(StardardMessages.UpdateByBeNull);
            }

            var userInfo = await _jobService.AcceptValidationResultAsync(acceptValidationFileRequest.JobId, acceptValidationFileRequest.JobFileId, acceptValidationFileRequest.UpdateBy);

            return Ok(userInfo);
        }
    }
}
