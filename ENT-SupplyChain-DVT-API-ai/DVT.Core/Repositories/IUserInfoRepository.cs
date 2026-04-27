using DVT.Core.Models;

namespace DVT.Core.Repositories
{
    public interface IUserInfoRepository : IRepository<UserInfo>
    {
        public ValueTask<UserInfo> GetByEmailAddressAsync(string emailAddress);
    }
}
