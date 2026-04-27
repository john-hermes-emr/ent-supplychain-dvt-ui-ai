using DVT.Core.Models;

namespace DVT.Core.Services
{
    public interface IUserInfoService
    {
        ValueTask<UserInfo> GetByIdAsync(Guid userId);
        ValueTask<UserInfo> GetByEmailAddressAsync(string emailAddress);
        ValueTask<UserInfo> SetFoldersAsync(UserInfo user);
        ValueTask<UserInfo> SetLoadFolderAsync(Guid userId, string loadPath, string updateBy);
        ValueTask<UserInfo> SetLogFolderAsync(Guid userId, string logPath, string updateBy);
        ValueTask<UserInfo> SetProductionFolderAsync(Guid userId, string prodPath, string updateBy);

    }
}
