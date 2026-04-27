using DVT.Core.Models;
using Microsoft.AspNetCore.SignalR;

namespace DVT.Core.Services
{
    public interface IRealtimeStatusReportService
    {
        Task SendJobStatusUpdate(JobStatusUpdate update);
        Task SendValidationStatusUpdate(JobStatusUpdate update);
    }
}
