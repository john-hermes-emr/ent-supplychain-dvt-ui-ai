using DVT.Core.Models;

namespace DVT.Core.Repositories
{
    public interface IActivityLogRepository:IRepository<ActivityLog>
    {
        ValueTask<IEnumerable<ActivityLog>> GetByEntityId(Guid entityid);
    }
}
