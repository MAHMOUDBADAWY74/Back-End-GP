using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineLibrary.Service.UserService.Dtos;
using OnlineLibrary.Service.UserService;
using OnlineLibrary.Service.HandleResponse;
using OnlineLibrary.Data.Contexts;
using OnlineLibrary.Data.Entities;

namespace OnlineLibrary.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly OnlineLibraryIdentityDbContext _dbContext;

        public AccountController(IUserService userService, OnlineLibraryIdentityDbContext dbContext)
        {
            _userService = userService;
            _dbContext = dbContext;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto input)
        {
            try
            {
                var user = _dbContext.Users.FirstOrDefault(u => u.Email == input.Email);
                if (user == null)
                {
                    return BadRequest(new
                    {
                        errors = new[] { "Invalid login attempt." },
                        details = (string)null,
                        statusCode = 400,
                        message = "Bad Request"
                    });
                }

                if (user.IsBlocked)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new
                    {
                        errors = new[] { "Your account has been blocked by the administration. Please contact support if you believe this is a mistake." },
                        details = (string)null,
                        statusCode = 403,
                        message = "Account Blocked"
                    });
                }

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

                _dbContext.Visits.Add(new Visit
                {
                    VisitDate = DateTime.UtcNow,
                    UserId = result.Id.ToString()
                });
                await _dbContext.SaveChangesAsync();

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

                _dbContext.Visits.Add(new Visit
                {
                    VisitDate = DateTime.UtcNow,
                    UserId = result.Id.ToString()
                });
                await _dbContext.SaveChangesAsync();

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
            var result = users.Select(u => new
            {
                userId = u.Id,
                userName = u.UserName,
                profilePicture = u.ProfilePicture
            });
            return Ok(result);
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
    }
}
