using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace DVT.Core.Services
{
    //[Authorize]
    public sealed class StatusNotificationHub : Hub<IStatusNotificationHubClient>
    {
        /// <summary>
        /// The client will call this method after they connect to the hub to register their interest in a specific job.
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        public async Task RegisterJobWithClient(Guid jobId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, jobId.ToString());
            Console.WriteLine($"Registered connection {Context.ConnectionId} for job {jobId}");
        }
    }
}
