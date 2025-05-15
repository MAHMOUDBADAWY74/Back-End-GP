using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using OnlineLibrary.Data.Contexts;
using OnlineLibrary.Data.Entities;
using OnlineLibrary.Service.CommunityService;
using OnlineLibrary.Service.CommunityService.Dtos;
using OnlineLibrary.Web.Hubs;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

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
            var community = await _communityService.CreateCommunityAsync(dto, userId);
            return Ok(community);
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

            string message = $"A new post has been added to the community by {userId}!";
            await _notificationHub.Clients.Group($"Community_{dto.CommunityId}")
                .SendAsync("ReceiveNotification", message);
            Console.WriteLine($"Sending notification to group Community_{dto.CommunityId}: {message}");

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

            string message = $"User {userId} liked your post!";
            await _notificationHub.Clients.User(postOwnerId).SendAsync("ReceiveNotification", message);
            Console.WriteLine($"Sending notification to {postOwnerId}: {message}");

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

            string message = $"User {userId} unliked your post!";
            await _notificationHub.Clients.User(postOwnerId).SendAsync("ReceiveNotification", message);
            Console.WriteLine($"Sending notification to {postOwnerId}: {message}");

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

            string message = $"User {userId} commented on your post!";
            await _notificationHub.Clients.User(postOwnerId).SendAsync("ReceiveNotification", message);
            Console.WriteLine($"Sending notification to {postOwnerId}: {message}");

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

            string message = $"User {userId} shared your post!";
            await _notificationHub.Clients.User(postOwnerId).SendAsync("ReceiveNotification", message);
            Console.WriteLine($"Sending notification to {postOwnerId}: {message}");

            return Ok();
        }

        [HttpPost("moderators/assign")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignModerator(AssignModeratorDto dto)
        {
            var adminId = GetUserId();
            await _communityService.AssignModeratorAsync(dto, adminId);

            string message = $"User {dto.UserId} has been assigned as a moderator in the community!";
            await _notificationHub.Clients.Group($"Community_{dto.CommunityId}")
                .SendAsync("ReceiveNotification", message);
            Console.WriteLine($"Sending notification to group Community_{dto.CommunityId}: {message}");

            return Ok();
        }

        [HttpPost("moderators/remove")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveModerator([FromQuery] long communityId, [FromQuery] string userId)
        {
            var adminId = GetUserId();
            await _communityService.RemoveModeratorAsync(communityId, userId, adminId);

            string message = $"User {userId} has been removed as a moderator from the community!";
            await _notificationHub.Clients.Group($"Community_{communityId}")
                .SendAsync("ReceiveNotification", message);
            Console.WriteLine($"Sending notification to group Community_{communityId}: {message}");

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