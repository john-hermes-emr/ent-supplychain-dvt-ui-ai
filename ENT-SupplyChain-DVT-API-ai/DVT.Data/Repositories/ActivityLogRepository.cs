using DVT.Core.Models;
using DVT.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DVT.Data.Repositories
{
    public class ActivityLogRepository:Repository<ActivityLog>, IActivityLogRepository
    {
        private DVTContext context;

        public ActivityLogRepository(DVTContext context) : base(context)
        {
            this.context = context;
        }

        public async ValueTask<IEnumerable<ActivityLog>> GetByEntityId(Guid entityid)
        {             
                return await context.ActivityLogs
                    .Where(log => log.EntityId == entityid && !log.Deleted)
                    .OrderByDescending(log => log.CreateDate)
                    .ToListAsync();            
        }
    }
}
