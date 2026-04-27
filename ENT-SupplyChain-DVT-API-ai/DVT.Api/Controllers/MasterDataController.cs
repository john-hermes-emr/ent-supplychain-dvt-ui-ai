using DVT.Api.Contracts;
using DVT.Api.Mapping;
using DVT.Api.Models;
using DVT.Core.Models;
using DVT.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using static DVT.Api.Contracts.ApiRoutes;
using static DVT.Core.Constants;

namespace DVT.Api.Controllers
{
    [Authorize]
    [EnableCors(ApiRoutes.SecurityDecorations.CorsAllowAll)]
    public class MasterDataController : ControllerBase
    {
        public readonly IMasterDataService _masterDataService;

        public MasterDataController(IMasterDataService masterDataService)
        {
            _masterDataService = masterDataService;
        }

        [HttpGet(ApiRoutes.MasterData.GetAllDivisions)]
        public async Task<ActionResult<IEnumerable<MasterDataDto>>> GetAllDivisionsAsync()
        {
            var divisions = await _masterDataService.GetAllDivisionsAsync();
            var mapper = new Mapper();
            var divisionsDtos = mapper.MasterDataToMasterDataDtos(divisions);
            return Ok(divisionsDtos);
        }

        [HttpGet(ApiRoutes.MasterData.GetAllMasterData)]
        public async Task<ActionResult<IEnumerable<MasterDataDto>>> GetAllMasterDataAsync()
        {
            var masterData = await _masterDataService.GetAllMasterDataAsync();
            var mapper = new Mapper();
            var masterDataDtos = mapper.MasterDataToMasterDataDtos(masterData);
            return Ok(masterDataDtos);
        }

        [HttpGet(ApiRoutes.MasterData.GetAllTableNames)]
        public async Task<ActionResult<IEnumerable<string>>> GetAllTableNamesAsync()
        {
            var tableNames = await _masterDataService.GetAllTableNamesAsync();
            return Ok(tableNames);
        }

        [HttpGet(ApiRoutes.MasterData.GetMasterDataByTableName)]
        public async Task<ActionResult<MasterDataDto>> GetMasterDataByTableNameAsync([FromRoute] string id)
        {
            var masterData = await _masterDataService.GetMasterDataByTableNamesAsync(id);
            var mapper = new Mapper();
            var masterDataDtos = mapper.MasterDataToMasterDataDtos(masterData);
            return Ok(masterDataDtos);
        }
    }
}
