using DVT.Api.Contracts;
using DVT.Api.Extensions;
using DVT.Api.Models;
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
    public class StorageController : ControllerBase
    {
        private readonly IStorageService _storageService;

        public StorageController(IStorageService storageService)
        {
            _storageService = storageService;
        }

        [HttpGet(ApiRoutes.Storages.GetFoldersFromUserShareByEmailAddress)]
        public async Task<ActionResult<FolderList>> GetFoldersFromUserSharebyEmailAddressAsync([FromRoute] string id)
        {
            //check current user email address
            var emailAddress = HttpContext.GetUserEmailFromHttpContext();

            if (!string.Equals(emailAddress, id, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("You are not authorized to access folders for this email address.");
            }

            var folders = await _storageService.GetFoldersByEmailAddressAsync(id);
            return Ok(folders);
        }

        [HttpPost(ApiRoutes.Storages.DownloadFileFromMainShareDocs)]
        public async Task<ActionResult> DownloadFileFromMainShareDocs([FromBody] DownloadFileRequest downloadFileRequest)
        {
            var name = downloadFileRequest.Name;
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(StardardMessages.ObjectCannotBeNull);
            }

            var fileEntity = await _storageService.GetMainShareDocsFileEntityAsync(name);

            return File(fileEntity.FileBytes, fileEntity.ContentType, name);
        }
    }
}
