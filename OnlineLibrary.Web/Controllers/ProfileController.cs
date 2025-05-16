using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineLibrary.Service.UserProfileService;
using OnlineLibrary.Service.UserProfileService.Dtos;
using System.Threading.Tasks;

namespace OnlineLibrary.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IUserProfile _userProfileService;

        public ProfileController(IUserProfile userProfileService)
        {
            _userProfileService = userProfileService;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var profile = await _userProfileService.GetProfileAsync(userId);
            return Ok(profile);
        }

        [HttpGet("profile/{profileId}")]
        public async Task<IActionResult> GetProfileById(long profileId)
        {
            if (profileId <= 0)
            {
                return BadRequest("Profile ID must be a positive number.");
            }

            var profile = await _userProfileService.GetProfileByIdAsync(profileId);
            return Ok(profile);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProfile([FromForm] UserProfileCreateDto profileDto)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var profile = await _userProfileService.CreateProfileAsync(userId, profileDto);
            return CreatedAtAction(nameof(GetMyProfile), new { id = userId }, profile);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromForm] UserProfileUpdateDto profileDto)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var profile = await _userProfileService.UpdateProfileAsync(userId, profileDto);
            return Ok(profile);
        }

        [HttpPut("profile-photo")]
        public async Task<IActionResult> UpdateProfilePhoto([FromForm] ProfilePhotoUpdateDto photoDto)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            if (photoDto.ProfilePhotoUpdate == null || photoDto.ProfilePhotoUpdate.Length == 0)
            {
                return BadRequest("Profile photo is required.");
            }

            var profile = await _userProfileService.UpdateProfilePhotoAsync(userId, photoDto.ProfilePhotoUpdate);
            return Ok(profile);
        }

        [HttpPut("cover-photo")]
        public async Task<IActionResult> UpdateCoverPhoto([FromForm] CoverPhotoUpdateDto photoDto)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            if (photoDto.CoverPhotoUpdate == null || photoDto.CoverPhotoUpdate.Length == 0)
            {
                return BadRequest("Cover photo is required.");
            }

            var profile = await _userProfileService.UpdateCoverPhotoAsync(userId, photoDto.CoverPhotoUpdate);
            return Ok(profile);
        }
    }
}