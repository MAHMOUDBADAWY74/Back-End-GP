using Microsoft.AspNetCore.SignalR;
using OnlineLibrary.Data.Contexts;
using OnlineLibrary.Data.Entities;
using OnlineLibrary.Web.Hubs.Dtos;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineLibrary.Web.Hubs
{
    public class NotificationHub : Hub
    {
        private readonly OnlineLibraryIdentityDbContext _dbContext;

        public NotificationHub(OnlineLibraryIdentityDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
            {
                throw new HubException("User not authenticated");
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, "AllUsers");

            var communityMemberships = _dbContext.CommunityMembers
                .Where(cm => cm.UserId == userId)
                .Select(cm => cm.CommunityId)
                .ToList();

            foreach (var communityId in communityMemberships)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"Community_{communityId}");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.User?.Identity?.Name;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, "AllUsers");

                var communityMemberships = _dbContext.CommunityMembers
                    .Where(cm => cm.UserId == userId)
                    .Select(cm => cm.CommunityId)
                    .ToList();

                foreach (var communityId in communityMemberships)
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Community_{communityId}");
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}