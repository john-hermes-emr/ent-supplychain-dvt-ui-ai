using DVT.Core.Models;
using DVT.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using static DVT.Core.Constants;

namespace DVT.Data.Repositories
{
    public class JobRepository : Repository<Job>, IJobRepository
    {
        private DVTContext context;

        public JobRepository(DVTContext context) : base(context)
        {
            this.context = context;
        }

        public async ValueTask<Job?> GetByIdAsync(Guid id)
        {
            return await context.Jobs
                .SingleOrDefaultAsync(job => job.JobId == id && !job.Deleted);
        }

        public async ValueTask<Job?> GetByIdNoTrackingAsync(Guid id)
        {
            return await context.Jobs
                .AsNoTracking()
                .SingleOrDefaultAsync(job => job.JobId == id && !job.Deleted);
        }

        public async ValueTask<Job?> GetUserLatestActiveJobAsync(Guid userInfoId)
        {
            return await context.Jobs
                .Include(x => x.JobFiles.Where(x => !x.Deleted))
                .Where(job => job.UserInfoId == userInfoId && !string.Equals(job.Status, WellKnownJobStatuses.Completed) && !job.Deleted)
                .OrderByDescending(x => x.UpdateDate)
                .FirstOrDefaultAsync();
        }
    }
}
