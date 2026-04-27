using DVT.Api.Contracts;
using DVT.Api.Mapping;
using DVT.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using static DVT.Core.Constants;

namespace DVT.Api.Controllers
{
    [Authorize]
    [EnableCors(ApiRoutes.SecurityDecorations.CorsAllowAll)]
    public class UserInfoController : ControllerBase
    {
        public readonly IUserInfoService _userInfoService;
        public UserInfoController(IUserInfoService userInfoService)
        {
            _userInfoService = userInfoService;
        }

        [HttpGet(ApiRoutes.UserInfos.GetById)]
        public async Task<ActionResult<UserInfoDto>> GetByIdAsync([FromRoute] Guid id)
        {
            if (id == null || id == Guid.Empty)
            {
                return NotFound(StardardMessages.ObjectCannotBeNull);
            }

            var userInfo = await _userInfoService.GetByIdAsync(id);
            var mapper = new Mapper();
            var userInfoDto = mapper.UserInfoToUserInfoDto(userInfo);
            return Ok(userInfoDto); 
        }

        [HttpGet(ApiRoutes.UserInfos.GetByEmailAddress)]
        public async Task<ActionResult<UserInfoDto>> GetByEmailAddressAsync([FromRoute] string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound(StardardMessages.ObjectCannotBeNull);
            }

            var userInfo = await _userInfoService.GetByEmailAddressAsync(id);
            var mapper = new Mapper();
            var userInfoDto = mapper.UserInfoToUserInfoDto(userInfo);
            return Ok(userInfoDto);
        }

        [HttpPost(ApiRoutes.UserInfos.SetFolders)]
        public async Task<ActionResult<UserInfoDto>> SetFoldersAsync([FromBody] UserInfoSetRequest userInfoSetRequest)
        {
            if (userInfoSetRequest == null)
            {
                return NotFound(StardardMessages.ObjectCannotBeNull);
            }

            if (userInfoSetRequest.UserInfoId == null || userInfoSetRequest.UserInfoId == Guid.Empty)
            {
                return NotFound(StardardMessages.UserInfoIdCannotBeNull);
            }

            if (string.IsNullOrWhiteSpace(userInfoSetRequest.LoadFolder))
            {
                return NotFound(StardardMessages.LoadDirectoryCannotBeEmpty);
            }

            if (string.IsNullOrWhiteSpace(userInfoSetRequest.LogFolder))
            {
                return NotFound(StardardMessages.LoadDirectoryCannotBeEmpty);
            }

            if (string.IsNullOrWhiteSpace(userInfoSetRequest.ProductionFolder))
            {
                return NotFound(StardardMessages.ProductionDirectoryCannotBeEmpty);
            }

            if (string.IsNullOrWhiteSpace(userInfoSetRequest.UpdateBy))
            {
                return NotFound(StardardMessages.UpdateByBeNull);
            }

            var mapper = new Mapper();
            var setUserInfo = mapper.UserInfoSetRequestToUserInfo(userInfoSetRequest);
            var userInfo = await _userInfoService.SetFoldersAsync(setUserInfo);
            
            var userInfoDto = mapper.UserInfoToUserInfoDto(userInfo);
            return Ok(userInfoDto);
        }

        [HttpPost(ApiRoutes.UserInfos.SetLoadFolder)]
        public async Task<ActionResult<UserInfoDto>> SetLoadFolderAsync([FromBody] UserInfoSetLoadFolderRequest userInfoSetRequest)
        {
            if (userInfoSetRequest == null)
            {
                return NotFound(StardardMessages.ObjectCannotBeNull);
            }

            if (userInfoSetRequest.UserInfoId == null || userInfoSetRequest.UserInfoId == Guid.Empty)
            {
                return NotFound(StardardMessages.UserInfoIdCannotBeNull);
            }

            if (string.IsNullOrWhiteSpace(userInfoSetRequest.LoadFolder))
            {
                return NotFound(StardardMessages.LoadDirectoryCannotBeEmpty);
            }

            if (string.IsNullOrWhiteSpace(userInfoSetRequest.UpdateBy))
            {
                return NotFound(StardardMessages.UpdateByBeNull);
            }

            var mapper = new Mapper();
            var userInfo = await _userInfoService.SetLoadFolderAsync(userInfoSetRequest.UserInfoId, userInfoSetRequest.LoadFolder, userInfoSetRequest.UpdateBy);

            var userInfoDto = mapper.UserInfoToUserInfoDto(userInfo);
            return Ok(userInfoDto);
        }

        [HttpPost(ApiRoutes.UserInfos.SetLogFolder)]
        public async Task<ActionResult<UserInfoDto>> SetLogFolderAsync([FromBody] UserInfoSetLogFolderRequest userInfoSetRequest)
        {
            if (userInfoSetRequest == null)
            {
                return NotFound(StardardMessages.ObjectCannotBeNull);
            }

            if (userInfoSetRequest.UserInfoId == null || userInfoSetRequest.UserInfoId == Guid.Empty)
            {
                return NotFound(StardardMessages.UserInfoIdCannotBeNull);
            }

            if (string.IsNullOrWhiteSpace(userInfoSetRequest.LogFolder))
            {
                return NotFound(StardardMessages.LogDirectoryCannotBeEmpty);
            }

            if (string.IsNullOrWhiteSpace(userInfoSetRequest.UpdateBy))
            {
                return NotFound(StardardMessages.UpdateByBeNull);
            }

            var mapper = new Mapper();
            var userInfo = await _userInfoService.SetLogFolderAsync(userInfoSetRequest.UserInfoId, userInfoSetRequest.LogFolder, userInfoSetRequest.UpdateBy);

            var userInfoDto = mapper.UserInfoToUserInfoDto(userInfo);
            return Ok(userInfoDto);
        }

        [HttpPost(ApiRoutes.UserInfos.SetProdFolder)]
        public async Task<ActionResult<UserInfoDto>> SetProdFolderAsync([FromBody] UserInfoSetProductionFolderRequest userInfoSetRequest)
        {
            if (userInfoSetRequest == null)
            {
                return NotFound(StardardMessages.ObjectCannotBeNull);
            }

            if (userInfoSetRequest.UserInfoId == null || userInfoSetRequest.UserInfoId == Guid.Empty)
            {
                return NotFound(StardardMessages.UserInfoIdCannotBeNull);
            }

            if (string.IsNullOrWhiteSpace(userInfoSetRequest.ProductionFolder))
            {
                return NotFound(StardardMessages.ProductionDirectoryCannotBeEmpty);
            }

            if (string.IsNullOrWhiteSpace(userInfoSetRequest.UpdateBy))
            {
                return NotFound(StardardMessages.UpdateByBeNull);
            }

            var mapper = new Mapper();
            var userInfo = await _userInfoService.SetProductionFolderAsync(userInfoSetRequest.UserInfoId, userInfoSetRequest.ProductionFolder, userInfoSetRequest.UpdateBy);

            var userInfoDto = mapper.UserInfoToUserInfoDto(userInfo);
            return Ok(userInfoDto);
        }
    }
}
