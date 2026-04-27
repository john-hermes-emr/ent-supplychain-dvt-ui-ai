using DVT.Core.Models;
using Microsoft.AspNetCore.SignalR;

namespace DVT.Core.Services
{
    public class RealtimeStatusReportService : IRealtimeStatusReportService
    {
        private readonly IHubContext<StatusNotificationHub, IStatusNotificationHubClient> _hubContext;

        public RealtimeStatusReportService(IHubContext<StatusNotificationHub, IStatusNotificationHubClient> hubContext)
        {
            _hubContext = hubContext;
        }
        public async Task SendJobStatusUpdate(JobStatusUpdate update)
        {
            await _hubContext.Clients.Groups(update.JobId.ToString()).ReceiveJobStatusUpdate(update);
        }

        public async Task SendValidationStatusUpdate(JobStatusUpdate update)
        {
            await _hubContext.Clients.Groups(update.JobId.ToString()).ReceiveValidationStatusUpdate(update);
        }
    }
}
