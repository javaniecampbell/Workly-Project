using Microsoft.AspNetCore.SignalR;
using Workly.Core.HubClient;

namespace Api.Hubs
{
    public class NotificationHub : Hub<INotificationClient>
    {
        public override Task OnConnectedAsync()
        {


            return base.OnConnectedAsync();
        }
    }
}
