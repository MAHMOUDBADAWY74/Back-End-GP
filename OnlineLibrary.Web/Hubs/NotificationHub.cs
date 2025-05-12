using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace OnlineLibrary.Web.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task SendNotification(string userId, string message)
        {
            Console.WriteLine($"Sending notification to {userId}: {message}");
            await Clients.User(userId).SendAsync("ReceiveNotification", message);
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.Identity?.Name; 
            if (string.IsNullOrEmpty(userId))
            {
                throw new HubException("User not authenticated");
            }
            await base.OnConnectedAsync();
        }
    }
}