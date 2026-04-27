using DVT.Api.Contracts;
using DVT.Api.Models;
using DVT.Core.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DVT.Api.Controllers
{
    [EnableCors(ApiRoutes.SecurityDecorations.CorsAllowAll)]
    public class AboutController : ControllerBase
    {
        private readonly IAboutService _aboutService;
        private readonly IDbHealthCheckService _dbHealthCheckService;

        public AboutController(IAboutService aboutService, IDbHealthCheckService dbHealthCheckService)
        {            
            _aboutService = aboutService;
            _dbHealthCheckService = dbHealthCheckService;
        }

        [HttpGet(ApiRoutes.About.Get)]
        public async Task<IActionResult> GetAbout()
        {
            var returnObject = _aboutService.GetAbout();
            return Ok(returnObject);            
        }

        [HttpGet(ApiRoutes.About.GetStartupInfo)]
        public async Task<IActionResult> GetStartupInfo()
        {
            var startupInfo = _aboutService.GetStartupInfo();
            return Ok(startupInfo);
        }

        [HttpGet(ApiRoutes.About.TestDatabaseConnection)]
        public async Task<IActionResult> TestDatabaseConnection()
        {
            var dbConnectionSuccess = await _dbHealthCheckService.TestDatabaseConnectionAsync();
            return Ok(dbConnectionSuccess);
        }

        [HttpGet(ApiRoutes.About.GetDatabaseTables)]
        public async Task<IActionResult> GetDatabaseTables()
        {
            var tableNames = await _dbHealthCheckService.GetDatabaseTablesAsync();
            return Ok(tableNames);
        }
    }
}