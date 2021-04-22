using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace ContosoCrafts.Web.Server.Hubs
{
    public class EventsHub : Hub
    {
        private readonly ILogger<EventsHub> logger;
        public EventsHub(ILogger<EventsHub> logger)
        {
            this.logger = logger;

        }
        public override Task OnConnectedAsync()
        {
            logger.LogInformation($"Connection ID =>{Context.ConnectionId}\n User =>{Context.UserIdentifier}");
            return base.OnConnectedAsync();
        }
    }
}