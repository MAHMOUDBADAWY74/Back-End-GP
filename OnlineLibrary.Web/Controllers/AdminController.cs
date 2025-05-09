using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineLibrary.Service.AdminService;
using OnlineLibrary.Service.AdminService.Dtos;
using OnlineLibrary.Service.HandleResponse;

namespace OnlineLibrary.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : BaseController
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
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
    }
}