using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineLibrary.Service.UserService.Dtos;
using OnlineLibrary.Service.UserService;
using OnlineLibrary.Service.HandleResponse;

namespace OnlineLibrary.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IUserService _userService;

        public AccountController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto input)
        {
            try
            {
                var result = await _userService.Login(input);
                if (result == null)
                {
                    return BadRequest(new
                    {
                        errors = new[] { "Invalid login attempt." },
                        details = (string)null,
                        statusCode = 400,
                        message = "Bad Request"
                    });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    errors = new[] { ex.Message }, 
                    details = (string)null,
                    statusCode = 400,
                    message = "Bad Request"
                });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto input)
        {
            try
            {
                var result = await _userService.Register(input);
                if (result == null)
                {
                    return BadRequest(new
                    {
                        errors = new[] { "Email is already taken." },
                        details = (string)null,
                        statusCode = 400,
                        message = "Bad Request"
                    });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    errors = new[] { ex.Message },
                    details = (string)null,
                    statusCode = 400,
                    message = "Bad Request"
                });
            }
        }
        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers([FromQuery] string term)
        {
            var users = await _userService.SearchUsersAsync(term);
            return Ok(users);
        }


        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var result = await _userService.Logout();
            return Ok(new { success = result });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto input)
        {
            var result = await _userService.ForgotPassword(input);
            return Ok(new { success = result });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto input)
        {
            var result = await _userService.ResetPassword(input);
            return Ok(new { success = result });
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto input)
        {
            var result = await _userService.VerifyEmail(input);
            return Ok(new { success = result });
        }
    }
}