using DVT.Api.Contracts;
using DVT.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DVT.Api.Controllers
{
    [Authorize]
    public class ConfigSettingController : ControllerBase
    {
        private readonly IConfigSettingService _configSettingService;

        public ConfigSettingController(IConfigSettingService configSettingService)
        {
            _configSettingService = configSettingService;
        }

        [HttpGet(ApiRoutes.ConfigSettings.GetHelpDocuments)]
        public async Task<ActionResult> GetByModule()
        {
            var configSettings = await _configSettingService.GetHelpDocumentsAsync();

            return Ok(configSettings);
        }
    }
}
