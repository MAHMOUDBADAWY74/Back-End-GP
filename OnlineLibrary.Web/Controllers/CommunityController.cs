using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Data.Contexts;
using OnlineLibrary.Data.Entities;
using OnlineLibrary.Service.CommunityService;
using OnlineLibrary.Service.CommunityService.Dtos;
using OnlineLibrary.Web.Hubs;
using OnlineLibrary.Web.Hubs.Dtos;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OnlineLibrary.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommunityController : BaseController
    {
        private readonly ICommunityService _communityService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<NotificationHub> _notificationHub;
        private readonly OnlineLibraryIdentityDbContext _dbContext;

        public CommunityController(
            ICommunityService communityService,
            UserManager<ApplicationUser> userManager,
            IHubContext<NotificationHub> notificationHub,
            OnlineLibraryIdentityDbContext dbContext)
        {
            _communityService = communityService;
            _userManager = userManager;
            _notificationHub = notificationHub;
            _dbContext = dbContext;
        }

        private string GetUserId() => _userManager.GetUserId(User);

        private async Task<(string Username, string ProfilePicture)> GetUserDetails(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var userProfile = await _dbContext.UserProfiles
                .FirstOrDefaultAsync(up => up.UserId == userId);

            string username = user != null ? $"{user.firstName} {user.LastName}" : "Unknown";
            string profilePicture = userProfile?.ProfilePhoto ?? "default_profile.jpg";

            return (username, profilePicture);
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<CommunityDto>>> GetAllCommunities()
        {
            var userId = GetUserId();
            var communities = await _communityService.GetAllCommunitiesAsync(userId);
            return Ok(communities);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<CommunityDto>> GetCommunity(long id)
        {
            var visit = new Visit
            {
                VisitDate = DateTime.UtcNow,
                UserId = User.Identity.IsAuthenticated ? User.FindFirst(ClaimTypes.NameIdentifier)?.Value : null
            };
            _dbContext.Visits.Add(visit);
            await _dbContext.SaveChangesAsync();

            var community = await _communityService.GetCommunityByIdAsync(id);
            if (community == null)
                return NotFound();

            var userId = GetUserId();
            var communityMembers = await _communityService.GetCommunityMembersAsync(id);
            community.IsMember = communityMembers.Any(m => m.UserId == userId);

            return Ok(community);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CommunityDto>> CreateCommunity(CreateCommunityDto dto)
        {
            var userId = GetUserId();
            try
            {
                var community = await _communityService.CreateCommunityAsync(dto, userId);
                return Ok(community);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }


        [HttpPost("{communityId}/join")]
        [Authorize]
        public async Task<IActionResult> JoinCommunity(long communityId)
        {
            var userId = GetUserId();
            await _communityService.JoinCommunityAsync(communityId, userId);
            return Ok();
        }

        [HttpPost("{communityId}/leave")]
        [Authorize]
        public async Task<IActionResult> LeaveCommunity(long communityId)
        {
            var userId = GetUserId();
            await _communityService.LeaveCommunityAsync(communityId, userId);
            return Ok();
        }

        [HttpPost("posts")]
        [Authorize(Roles = "Sender,Admin,Moderator,User")]
        public async Task<ActionResult<CommunityPostDto>> CreatePost([FromForm] CreatePostDto dto)
        {
            var userId = GetUserId();
            var post = await _communityService.CreatePostAsync(dto, userId);

            var (username, profilePicture) = await GetUserDetails(userId);
            var notification = new NotificationDto
            {
                UserId = userId,
                Username = username,
                ProfilePicture = profilePicture,
                Text = $"{username} added a new post to the community!",
                Time = DateTime.UtcNow
            };

            await _notificationHub.Clients.GroupExcept($"Community_{dto.CommunityId}", userId)
                .SendAsync("ReceiveNotification", notification);
            Console.WriteLine($"Sending notification to group Community_{dto.CommunityId}: {notification.Text}");

            return Ok(post);
        }

        [HttpGet("{communityId}/posts")]
        [Authorize(Roles = "Receiver,Admin,Moderator,User")]
        public async Task<ActionResult<IEnumerable<CommunityPostDto>>> GetCommunityPosts(long communityId)
        {
            var userId = GetUserId();
            var posts = await _communityService.GetCommunityPostsAsync(communityId, userId);
            return Ok(posts);
        }

        [HttpGet("all-posts")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<CommunityPostDto>>> GetAllPosts([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            var userId = GetUserId();
            var posts = await _communityService.GetAllCommunityPostsAsync(pageNumber, pageSize, userId);
            return Ok(posts);
        }

        [HttpPost("posts/{postId}/like")]
        [Authorize]
        public async Task<IActionResult> LikePost(long postId)
        {
            var userId = GetUserId();
            string postOwnerId = await GetPostOwnerId(postId);
            if (string.IsNullOrEmpty(postOwnerId))
            {
                return NotFound("Post not found");
            }

            await _communityService.LikePostAsync(postId, userId);

            if (userId != postOwnerId) 
            {
                var (username, profilePicture) = await GetUserDetails(userId);
                var notification = new NotificationDto
                {
                    UserId = userId,
                    Username = username,
                    ProfilePicture = profilePicture,
                    Text = $"{username} liked your post!",
                    Time = DateTime.UtcNow
                };

                await _notificationHub.Clients.User(postOwnerId)
                    .SendAsync("ReceiveNotification", notification);
                Console.WriteLine($"Sending notification to {postOwnerId}: {notification.Text}");
            }

            return Ok();
        }

        [HttpPost("posts/{postId}/unlike")]
        [Authorize]
        public async Task<IActionResult> UnlikePost(long postId)
        {
            var userId = GetUserId();
            string postOwnerId = await GetPostOwnerId(postId);
            if (string.IsNullOrEmpty(postOwnerId))
            {
                return NotFound("Post not found");
            }

            await _communityService.UnlikePostAsync(postId, userId);

            if (userId != postOwnerId) 
            {
                var (username, profilePicture) = await GetUserDetails(userId);
                var notification = new NotificationDto
                {
                    UserId = userId,
                    Username = username,
                    ProfilePicture = profilePicture,
                    Text = $"{username} unliked your post!",
                    Time = DateTime.UtcNow
                };

                await _notificationHub.Clients.User(postOwnerId)
                    .SendAsync("ReceiveNotification", notification);
                Console.WriteLine($"Sending notification to {postOwnerId}: {notification.Text}");
            }

            return Ok();
        }

        [HttpPost("posts/comments")]
        [Authorize]
        public async Task<ActionResult<PostCommentDto>> AddComment(CreateCommentDto dto)
        {
            var userId = GetUserId();
            string postOwnerId = await GetPostOwnerId(dto.PostId);
            if (string.IsNullOrEmpty(postOwnerId))
            {
                return NotFound("Post not found");
            }

            var comment = await _communityService.AddCommentAsync(dto, userId);

            if (userId != postOwnerId) 
            {
                var (username, profilePicture) = await GetUserDetails(userId);
                var notification = new NotificationDto
                {
                    UserId = userId,
                    Username = username,
                    ProfilePicture = profilePicture,
                    Text = $"{username} commented on your post!",
                    Time = DateTime.UtcNow
                };

                await _notificationHub.Clients.User(postOwnerId)
                    .SendAsync("ReceiveNotification", notification);
                Console.WriteLine($"Sending notification to {postOwnerId}: {notification.Text}");
            }

            return Ok(comment);
        }

        [HttpGet("posts/{postId}/comments")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<PostCommentDto>>> GetPostComments(long postId)
        {
            var comments = await _communityService.GetPostCommentsAsync(postId);
            return Ok(comments);
        }

        [HttpPost("posts/{postId}/share")]
        [Authorize]
        public async Task<IActionResult> SharePost(long postId, [FromQuery] long? communityId)
        {
            var userId = GetUserId();
            string postOwnerId = await GetPostOwnerId(postId);
            if (string.IsNullOrEmpty(postOwnerId))
            {
                return NotFound("Post not found");
            }

            await _communityService.SharePostAsync(postId, userId, communityId);

            if (userId != postOwnerId) 
            {
                var (username, profilePicture) = await GetUserDetails(userId);
                var notification = new NotificationDto
                {
                    UserId = userId,
                    Username = username,
                    ProfilePicture = profilePicture,
                    Text = $"{username} shared your post!",
                    Time = DateTime.UtcNow
                };

                await _notificationHub.Clients.User(postOwnerId)
                    .SendAsync("ReceiveNotification", notification);
                Console.WriteLine($"Sending notification to {postOwnerId}: {notification.Text}");
            }

            return Ok();
        }

        [HttpPost("moderators/assign")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignModerator(AssignModeratorDto dto)
        {
            var adminId = GetUserId();
            await _communityService.AssignModeratorAsync(dto, adminId);

            var (username, profilePicture) = await GetUserDetails(dto.UserId);
            var notification = new NotificationDto
            {
                UserId = adminId,
                Username = username,
                ProfilePicture = profilePicture,
                Text = $"{username} has been assigned as a moderator in the community!",
                Time = DateTime.UtcNow
            };

            await _notificationHub.Clients.GroupExcept($"Community_{dto.CommunityId}", adminId)
                .SendAsync("ReceiveNotification", notification);
            Console.WriteLine($"Sending notification to group Community_{dto.CommunityId}: {notification.Text}");

            return Ok();
        }
        [HttpGet("moderators/all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllModerators()
        {
            var moderators = await _communityService.GetAllModeratorsAsync();
            return Ok(moderators);
        }



        [HttpPost("moderators/remove")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveModerator([FromQuery] long communityId, [FromQuery] string userId)
        {
            var adminId = GetUserId();
            await _communityService.RemoveModeratorAsync(communityId, userId, adminId);

            var (username, profilePicture) = await GetUserDetails(userId);
            var notification = new NotificationDto
            {
                UserId = adminId,
                Username = username,
                ProfilePicture = profilePicture,
                Text = $"{username} has been removed as a moderator from the community!",
                Time = DateTime.UtcNow
            };

            await _notificationHub.Clients.GroupExcept($"Community_{communityId}", adminId)
                .SendAsync("ReceiveNotification", notification);
            Console.WriteLine($"Sending notification to group Community_{communityId}: {notification.Text}");

            return Ok();
        }

        [HttpDelete("posts/{postId}")]
        [Authorize(Roles = "Admin,User,Moderator")]
        public async Task<IActionResult> DeletePost(long postId)
        {
            var requesterId = GetUserId();
            await _communityService.DeletePostAsync(postId, requesterId);
            return Ok();
        }

        [HttpDelete("comments/{commentId}")]
        [Authorize]
        public async Task<IActionResult> DeleteComment(long commentId)
        {
            var requesterId = GetUserId();
            await _communityService.DeleteCommentAsync(commentId, requesterId);
            return Ok();
        }

        [HttpPost("{communityId}/ban/{userId}")]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> BanUser(long communityId, string userId)
        {
            var requesterId = GetUserId();
            await _communityService.BanUserAsync(communityId, userId, requesterId);
            return Ok();
        }

        [HttpPost("{communityId}/unban/{userId}")]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> UnbanUser(long communityId, string userId)
        {
            var requesterId = GetUserId();
            var result = await _communityService.UnbanUserAsync(communityId, requesterId, userId);
            return Ok(new { Unbanned = result });
        }

        private async Task<string> GetPostOwnerId(long postId)
        {
            var post = await _dbContext.CommunityPosts.FindAsync(postId);
            return post?.UserId ?? string.Empty;
        }
    }
}