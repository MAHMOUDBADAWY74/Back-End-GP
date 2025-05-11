using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineLibrary.Service.UserService.Dtos;
using OnlineLibrary.Service.UserService;
using OnlineLibrary.Service.HandleResponse;

namespace OnlineLibrary.Web.Controllers
{
    public class AccountController : BaseController
    {
        private readonly IUserService _userService;

        public AccountController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<ActionResult<UserDto>> Login(LoginDto input)
        {
            var user = await _userService.Login(input);
            if (user == null)
                return BadRequest(new UserException(400, "Email Does Not Found"));
            return Ok(user);
        }

        [HttpPost]
        public async Task<ActionResult<UserDto>> Register(RegisterDto input)
        {
            Console.WriteLine($"Registering user: Email={input.Email}, FirstName={input.FirstName}, LastName={input.LastName}");
            var user = await _userService.Register(input);
            if (user == null)
                return BadRequest(new UserException(400, "Email Already Exists"));
            Console.WriteLine($"User registered: FirstName={user.FirstName}, LastName={user.LastName}");
            return Ok(user);
        }

        [HttpPost]
        public async Task<ActionResult> VerifyEmail(VerifyEmailDto input)
        {
            var result = await _userService.VerifyEmail(input);
            if (!result)
                return BadRequest(new UserException(400, "Email Verification Failed"));

            return Ok(new { Message = "Email Verified Successfully" });
        }

        [HttpPost]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordDto input)
        {
            var result = await _userService.ForgotPassword(input);
            if (!result)
                return BadRequest(new UserException(400, "Email Not Found"));

            return Ok(new { Message = "Password Reset Token Sent to Email" });
        }

        [HttpPost]
        public async Task<ActionResult> ResetPassword(ResetPasswordDto input)
        {
            var result = await _userService.ResetPassword(input);
            if (!result)
                return BadRequest(new UserException(400, "Password Reset Failed"));

            return Ok(new { Message = "Done" });
        }

        [HttpPost]
        public async Task<ActionResult> Logout()
        {
            var logoutSuccess = await _userService.Logout();
            if (!logoutSuccess)
            {
                return BadRequest("Logout failed");
            }
            return Ok("Successfully logged out");
        }

        [HttpPost]
        public async Task<IActionResult> RequestEditUser([FromBody] RequestEditUserDto request)
        {
            var result = await _userService.RequestEditUser(request.UserId, request.FieldName, request.NewValue);
            if (!result)
                return BadRequest("Failed to request edit.");

            return Ok("Edit request submitted.");
        }

        [HttpPost("request-delete")]
        public async Task<IActionResult> RequestDeleteUser([FromBody] RequestDeleteUserDto request)
        {
            var result = await _userService.RequestDeleteUser(request.UserId);
            if (!result)
                return BadRequest("Failed to request delete.");

            return Ok("Delete request submitted.");
        }
    }
}