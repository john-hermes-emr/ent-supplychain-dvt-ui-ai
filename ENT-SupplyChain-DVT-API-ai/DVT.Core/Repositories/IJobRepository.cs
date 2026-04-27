using DVT.Core.Models;

namespace DVT.Core.Repositories
{
    public interface IJobRepository : IRepository<Job>
    {
        ValueTask<Job> GetUserLatestActiveJobAsync(Guid userInfoId);

        ValueTask<Job> GetByIdAsync(Guid id);

        ValueTask<Job> GetByIdNoTrackingAsync(Guid id);
    }
}
