using OnlineLibrary.Service.UserService.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Service.UserService
{
    public interface IUserService
    {
        Task<UserDto> Login(LoginDto input);
        Task<UserDto> Register(RegisterDto input);

        Task<bool> VerifyEmail(VerifyEmailDto input);
        Task<bool> ForgotPassword(ForgotPasswordDto input);
        Task<bool> ResetPassword(ResetPasswordDto input);
        Task<bool> Logout();


        Task<bool> RequestEditUser(string userId, string fieldName, string newValue);
        Task<bool> RequestDeleteUser(string userId);
    }
}
