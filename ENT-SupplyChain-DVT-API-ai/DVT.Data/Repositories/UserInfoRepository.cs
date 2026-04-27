using DVT.Core.Models;
using DVT.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DVT.Data.Repositories
{
    public class UserInfoRepository : Repository<UserInfo>, IUserInfoRepository
    {
        private DVTContext context;

        public UserInfoRepository(DVTContext context) : base(context)
        {
            this.context = context;
        }

        public async ValueTask<UserInfo> GetByIdAsync(Guid id)
        {
            return await context.UserInfos
                .SingleOrDefaultAsync(x => x.UserInfoId == id && !x.Deleted);
        }

        public async ValueTask<UserInfo> GetByEmailAddressAsync(string emailAddress)
        {
            return await context.UserInfos
                .FirstOrDefaultAsync(u => string.Equals(u.EmailAddress.ToLower(), emailAddress.ToLower()) && !u.Deleted);
        }
    }
}
