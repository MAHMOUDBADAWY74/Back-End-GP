using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineLibrary.Service.AdminService.Dtos;
using OnlineLibrary.Service.AdminService;

namespace OnlineLibrary.Web.Controllers
{
    
    public class AdminController : BaseController
    {

        
    private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet("pending-changes")]
        public async Task<IActionResult> GetPendingChanges()
        {
            var changes = await _adminService.GetPendingChanges();
            return Ok(changes);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveChange([FromBody] ApproveChangeDto request)
        {
            var result = await _adminService.ApproveChange(request.ChangeId);
            if (!result)
                return BadRequest("Failed to approve change.");

            return Ok("Change approved.");
        }

        [HttpPost]
        public async Task<IActionResult> RejectChange([FromBody] RejectChangeDto request)
        {
            var result = await _adminService.RejectChange(request.ChangeId);
            if (!result)
                return BadRequest("Failed to reject change.");

            return Ok("Change rejected.");
        }
    }
}
