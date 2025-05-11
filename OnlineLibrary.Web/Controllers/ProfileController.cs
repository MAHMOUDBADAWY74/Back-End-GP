using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineLibrary.Data.Entities;
using OnlineLibrary.Service.UserProfileService;
using OnlineLibrary.Service.UserProfileService.Dtos;
using Microsoft.AspNetCore.Authorization;

namespace OnlineLibrary.Web.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : BaseController
    {
        private readonly IUserProfile _profileService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(
            IUserProfile profileService,
            UserManager<ApplicationUser> userManager)
        {
            _profileService = profileService;
            _userManager = userManager;
        }

        [HttpGet("me")]
        public async Task<ActionResult<UserProfileDto>> GetMyProfile()
        {
            var userId = _userManager.GetUserId(User);
            var profile = await _profileService.GetProfileAsync(userId);
            return Ok(profile);
        }

        [HttpPost]
        public async Task<ActionResult<UserProfileDto>> CreateProfile([FromForm] UserProfileCreateDto profileDto)
        {
            var userId = _userManager.GetUserId(User);
            var profile = await _profileService.CreateProfileAsync(userId, profileDto);
            return CreatedAtAction(nameof(GetMyProfile), profile);
        }

        [HttpPut]
        public async Task<ActionResult<UserProfileDto>> UpdateProfile([FromForm] UserProfileUpdateDto profileDto)
        {
            var userId = _userManager.GetUserId(User);
            var profile = await _profileService.UpdateProfileAsync(userId, profileDto);
            return Ok(profile);
        }
    }
}