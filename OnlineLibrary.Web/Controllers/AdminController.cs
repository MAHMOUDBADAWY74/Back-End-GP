using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineLibrary.Service.AdminService;
using OnlineLibrary.Service.AdminService.Dtos;
using OnlineLibrary.Service.CommunityService;
using OnlineLibrary.Service.HandleResponse;
using OnlineLibrary.Service.UserService;
using OnlineLibrary.Data.Contexts;
using OnlineLibrary.Data.Entities;
using System.Threading.Tasks;

namespace OnlineLibrary.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : BaseController
    {
        private readonly IAdminService _adminService;
        private readonly IUserService _userService;
        private readonly ICommunityService _communityService;
        private readonly OnlineLibraryIdentityDbContext _dbContext;

        public AdminController(
            IAdminService adminService,
            IUserService userService,
            ICommunityService communityService,
            OnlineLibraryIdentityDbContext dbContext)
        {
            _adminService = adminService;
            _userService = userService;
            _communityService = communityService;
            _dbContext = dbContext;
        }

        [HttpPut("users/{userId}/roles")]
        public async Task<IActionResult> ChangeUserRole(string userId, [FromBody] ChangeRoleDto input)
        {
            try
            {
                var result = await _adminService.ChangeUserRole(userId, input);
                if (!result)
                    return BadRequest(new UserException(400, "Failed to change user role."));

                return Ok(new { Message = $"User role changed to {input.NewRole} successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new UserException(400, ex.Message));
            }
        }

        [HttpGet("pending-changes")]
        public async Task<IActionResult> GetPendingChanges()
        {
            try
            {
                var changes = await _adminService.GetPendingChanges();
                return Ok(changes);
            }
            catch (Exception ex)
            {
                return BadRequest(new UserException(400, ex.Message));
            }
        }

        [HttpPost("approve-change")]
        public async Task<IActionResult> ApproveChange([FromBody] ApproveChangeDto request)
        {
            try
            {
                var result = await _adminService.ApproveChange(request.ChangeId);
                if (!result)
                    return BadRequest(new UserException(400, "Failed to approve change."));

                return Ok(new { Message = "Change approved successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new UserException(400, ex.Message));
            }
        }

        [HttpPost("reject-change")]
        public async Task<IActionResult> RejectChange([FromBody] RejectChangeDto request)
        {
            try
            {
                var result = await _adminService.RejectChange(request.ChangeId);
                if (!result)
                    return BadRequest(new UserException(400, "Failed to reject change."));

                return Ok(new { Message = "Change rejected successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new UserException(400, ex.Message));
            }
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersForAdminAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest(new UserException(400, ex.Message));
            }
        }

        [HttpGet("posts/summary")]
        public async Task<IActionResult> GetAllCommunityPostsSummary([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var posts = await _communityService.GetAllCommunityPostsSummaryAsync(pageNumber, pageSize);
                return Ok(new
                {
                    pageNumber,
                    pageSize,
                    posts
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new UserException(400, ex.Message));
            }
        }

        [HttpGet("community-posts-count")]
        public async Task<IActionResult> GetCommunityPostsCount()
        {
            try
            {
                var communityPostsCount = await _communityService.GetCommunityPostsCountAsync();
                return Ok(communityPostsCount);
            }
            catch (Exception ex)
            {
                return BadRequest(new UserException(400, ex.Message));
            }
        }

        [HttpGet("communities")]
        public async Task<IActionResult> GetAllCommunitiesSummary()
        {
            try
            {
                var communities = await _communityService.GetAllCommunitiesSummaryAsync();
                return Ok(communities);
            }
            catch (Exception ex)
            {
                return BadRequest(new UserException(400, ex.Message));
            }
        }

        [HttpGet("visits/monthly/{year}")]
        public async Task<IActionResult> GetMonthlyVisits(int year)
        {
            try
            {
                var monthlyVisits = await _communityService.GetMonthlyVisitsAsync(year);
                return Ok(monthlyVisits);
            }
            catch (Exception ex)
            {
                return BadRequest(new UserException(400, ex.Message));
            }
        }

        [HttpPost("ban-site/{userId}")]
        public async Task<IActionResult> BanUserSiteWide(string userId)
        {
            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
                return NotFound("User not found.");

            user.IsBlocked = true;
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "User blocked site-wide." });
        }

        [HttpPost("unban-site/{userId}")]
        public async Task<IActionResult> UnbanUserSiteWide(string userId)
        {
            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
                return NotFound("User not found.");

            user.IsBlocked = false;
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "User unblocked site-wide." });
        }
    }
}
