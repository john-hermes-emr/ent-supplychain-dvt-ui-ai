using DVT.Api.Contracts;
using DVT.Api.Contracts.Job;
using DVT.Core.Models;
using Riok.Mapperly.Abstractions;
using System.Collections;

namespace DVT.Api.Mapping
{
    [Mapper]
    public partial class Mapper
    {
        public partial UserInfoDto UserInfoToUserInfoDto(UserInfo userInfo);

        public partial UserInfo UserInfoSetRequestToUserInfo(UserInfoSetRequest userInfoSetRequest);

        public partial Job JobCreateRequestToJob(JobCreateRequest jobCreateRequest);

        public partial MasterDataDto MasterDataToMasterDataDto(MasterData masterData);
        public partial IEnumerable<MasterDataDto> MasterDataToMasterDataDtos(IEnumerable<MasterData> masterData);

    }
}