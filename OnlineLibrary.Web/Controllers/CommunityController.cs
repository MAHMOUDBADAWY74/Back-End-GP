using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineLibrary.Data.Entities;
using OnlineLibrary.Service.CommunityService;
using OnlineLibrary.Service.CommunityService.Dtos;
using System.Security.Claims;

namespace OnlineLibrary.Web.Controllers
{
   
    [ApiController]
    public class CommunityController : BaseController
    {

        private readonly ICommunityService _communityService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CommunityController(
            ICommunityService communityService,
            UserManager<ApplicationUser> userManager)
        {
            _communityService = communityService;
            _userManager = userManager;
        }

        private string GetUserId() => _userManager.GetUserId(User);

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CommunityDto>>> GetAllCommunities()
        {
            var communities = await _communityService.GetAllCommunitiesAsync();
            return Ok(communities);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CommunityDto>> GetCommunity(long id)
        {
            var community = await _communityService.GetCommunityByIdAsync(id);
            if (community == null)
                return NotFound();

            return Ok(community);

        }
        [HttpPost]
        public async Task<ActionResult<CommunityDto>> CreateCommunity(CreateCommunityDto dto)
        {
            var userId = GetUserId();
            var community = await _communityService.CreateCommunityAsync(dto, userId);
            return Ok(community);
        }
        [HttpPost("{communityId}")]
        public async Task<IActionResult> JoinCommunity(long communityId)
        {
            var userId = GetUserId();
            await _communityService.JoinCommunityAsync(communityId, userId);
            return Ok();
        }

        [HttpPost("{communityId}")]
        public async Task<IActionResult> LeaveCommunity(long communityId)
        {
            var userId = GetUserId();
            await _communityService.LeaveCommunityAsync(communityId, userId);
            return Ok();
        }
    
        [HttpPost]
        public async Task<ActionResult<CommunityPostDto>> CreatePost(CreatePostDto dto)
        {
            var userId = GetUserId();
            var post = await _communityService.CreatePostAsync(dto, userId);
            return Ok(post);
        }

        [HttpGet("{communityId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CommunityPostDto>>> GetCommunityPosts(long communityId)
        {
            var userId = GetUserId(); 
            var posts = await _communityService.GetCommunityPostsAsync(communityId, userId);
            return Ok(posts);
        }

        [HttpPost("{postId}")]
        public async Task<IActionResult> LikePost(long postId)
        {
            var userId = GetUserId();
            await _communityService.LikePostAsync(postId, userId);
            return Ok();
        }

        [HttpPost("{postId}")]
        public async Task<IActionResult> UnlikePost(long postId)
        {
            var userId = GetUserId();
            await _communityService.UnlikePostAsync(postId, userId);
            return Ok();
        }

        [HttpPost]
        public async Task<ActionResult<PostCommentDto>> AddComment(CreateCommentDto dto)
        {
            var userId = GetUserId();
            var comment = await _communityService.AddCommentAsync(dto, userId);
            return Ok(comment);
        }

        [HttpGet("{postId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<PostCommentDto>>> GetPostComments(long postId)
        {
            var comments = await _communityService.GetPostCommentsAsync(postId);
            return Ok(comments);
        }

        [HttpPost("posts/{postId}/share")]
        public async Task<IActionResult> SharePost(long postId, [FromQuery] long? communityId)
        {
            var userId = GetUserId();
            await _communityService.SharePostAsync(postId, userId, communityId);
            return Ok();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")] 
        public async Task<IActionResult> AssignModerator(AssignModeratorDto dto)
        {
            var adminId = GetUserId();
            await _communityService.AssignModeratorAsync(dto, adminId);
            return Ok();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")] 
        public async Task<IActionResult> RemoveModerator([FromQuery] long communityId, [FromQuery] string userId)
        {
            var adminId = GetUserId();
            await _communityService.RemoveModeratorAsync(communityId, userId, adminId);
            return Ok();
        }

        [HttpDelete("posts/{postId}")]
        public async Task<IActionResult> DeletePost(long postId)
        {
            var requesterId = GetUserId();
            await _communityService.DeletePostAsync(postId, requesterId);
            return Ok();
        }

        [HttpDelete("comments/{commentId}")]
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


    }
}

