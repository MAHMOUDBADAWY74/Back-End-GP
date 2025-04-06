using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Data.Contexts;
using OnlineLibrary.Data.Entities;
using OnlineLibrary.Service.TokenService;
using OnlineLibrary.Service.UserService.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Service.UserService
{
    public class UserService : IUserService
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly OnlineLibraryIdentityDbContext _context;

        public UserService(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ITokenService tokenService,
            OnlineLibraryIdentityDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _tokenService = tokenService;
            _context = context;
        }

        public async Task<bool> ForgotPassword(ForgotPasswordDto input)
        {
            var user = await _userManager.FindByEmailAsync(input.Email);
            if (user == null)
                throw new Exception("Email not found.");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            // الجديد
            Console.WriteLine($"Password Reset Token: {token}");
            return true;
        }

        public async Task<UserDto> Login(LoginDto input)
        {
            var user = await _userManager.FindByEmailAsync(input.Email);
            if (user == null)
                return null;

            var result = await _signInManager.CheckPasswordSignInAsync(user, input.Password, false);

            if (!result.Succeeded)
                throw new Exception("User Not Found");

            return new UserDto
            {
                Id = Guid.Parse(user.Id),
                UserName = user.UserName, 
                Email = user.Email!,
                Token = _tokenService.GenerateToken(user),
            };
        }

        public async Task<UserDto> Register(RegisterDto input)
        {
            var user = await _userManager.FindByEmailAsync(input.Email);

            if (user is not null)
                return null;
            var appUser = new ApplicationUser
            {
                firstName = input.FirstName,
                Email = input.Email,
                UserName = $"{input.FirstName}{input.LastName}",

            };

            var result = await _userManager.CreateAsync(appUser, input.Password);
            if (!result.Succeeded) throw new Exception(result.Errors.Select(x => x.Description).FirstOrDefault());

            return new UserDto
            {
                Id = Guid.Parse(appUser.Id),
                FirstName = appUser.firstName,
                Email = appUser.Email!,
                Token = _tokenService.GenerateToken(appUser),
            };
        }

        public async Task<bool> ResetPassword(ResetPasswordDto input)
        {
            var user = await _userManager.FindByIdAsync(input.Id.ToString());
            if (user == null)
                throw new Exception("Invalid User.");

            var result = await _userManager.ResetPasswordAsync(user, input.Token, input.NewPassword);
            if (!result.Succeeded)
                throw new Exception("Password reset failed.");

            return true;
        }


        public async Task<bool> VerifyEmail(VerifyEmailDto input)
        {
            var user = await _userManager.FindByIdAsync(input.Id.ToString());
            if (user == null)
                throw new Exception("Invalid User.");

            var result = await _userManager.ConfirmEmailAsync(user, input.Token);
            if (!result.Succeeded)
                throw new Exception("Email verification failed.");

            return true;
        }

        public async Task<bool> Logout()
        {
            await _signInManager.SignOutAsync(); 
            return true;
        }

        public async Task<bool> RequestEditUser(string userId, string fieldName, string newValue)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found.");

            var oldValue = GetPropertyValue(user, fieldName);

            var pendingChange = new PendingUserChange
            {
                UserId = userId,
                FieldName = fieldName,
                OldValue = oldValue,
                NewValue = newValue,
                ChangeRequestedAt = DateTime.UtcNow,
                IsApproved = false
            };

            _context.PendingUserChanges.Add(pendingChange);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RequestDeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found.");

            var pendingChange = new PendingUserChange
            {
                UserId = userId,
                FieldName = "Delete",
                OldValue = user.Id,
                NewValue = null,
                ChangeRequestedAt = DateTime.UtcNow,
                IsApproved = false
            };

            _context.PendingUserChanges.Add(pendingChange);
            await _context.SaveChangesAsync();

            return true;
        }


        private string GetPropertyValue(ApplicationUser user, string propertyName)
        {
            var property = user.GetType().GetProperty(propertyName);
            if (property == null)
                throw new Exception("Invalid property name.");

            return property.GetValue(user)?.ToString();
        }
    }
}

