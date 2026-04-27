using DVT.Core.Models;

namespace DVT.Core.Services
{
    public interface IActivityLogService
    {
        Task AddLogAsync(ActivityLog log);
        Task AddLogsAsync(List<ActivityLog> logs);
        ValueTask<IEnumerable<ActivityLog>> GetByEntityIdAsync(Guid id);
    }
}
