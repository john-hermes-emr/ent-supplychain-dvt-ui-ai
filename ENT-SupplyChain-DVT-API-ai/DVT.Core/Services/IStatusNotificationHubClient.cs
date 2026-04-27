using DVT.Core.Models;

namespace DVT.Core.Services
{
    public interface IStatusNotificationHubClient
    {
        Task ReceiveJobStatusUpdate(JobStatusUpdate update);
        Task ReceiveValidationStatusUpdate(JobStatusUpdate update);
        Task RegisterJobWithClient(Guid jobId, string connectionId);
    }
}
