using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Data.Contexts;
using OnlineLibrary.Data.Entities;
using OnlineLibrary.Service.ExchangeRequestService.DTOS;
using OnlineLibrary.Service.TokenService;
using OnlineLibrary.Service.UserService.Dtos;
using OnlineLibrary.Service.AdminService.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineLibrary.Service.UserService
{
    public class UserService : IUserService
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly OnlineLibraryIdentityDbContext _context;
        private readonly string _baseImageUrl; 

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
            Console.WriteLine($"Password Reset Token: {token}");
            return true;
        }

        public async Task<UserDto> Login(LoginDto input)
        {
            
            var user = await _userManager.Users
                .Include(u => u.UserProfile) 
                .FirstOrDefaultAsync(u => u.Email == input.Email);
            if (user == null)
                return null;

            var result = await _signInManager.CheckPasswordSignInAsync(user, input.Password, false);

            if (!result.Succeeded)
                throw new Exception("User Not Found");

            if (user.IsBlocked)
                throw new Exception("User is blocked and cannot log in.");

            
            string? profilePicture = null;
            if (user.UserProfile?.ProfilePhoto != null)
            {
                profilePicture = $"{_baseImageUrl}{user.UserProfile.ProfilePhoto}";
            }

            return new UserDto
            {
                Id = Guid.Parse(user.Id),
                FirstName = user.firstName,
                LastName = user.LastName,
                UserName = user.UserName,
                Email = user.Email,
                Gender = user.Gender,
                Age = user.DateOfBirth.HasValue ? CalculateAge(user.DateOfBirth) : null,
                Token = await _tokenService.GenerateJwtToken(user),
                ProfilePicture = profilePicture 
            };
        }

        public async Task<UserDto> Register(RegisterDto input)
        {
            var userByEmail = await _userManager.FindByEmailAsync(input.Email);
            if (userByEmail is not null)
                throw new Exception("Email is already taken.");

            var userByName = await _userManager.FindByNameAsync(input.UserName);
            if (userByName is not null)
                throw new Exception("Username is already taken.");

            var appUser = new ApplicationUser
            {
                firstName = input.FirstName,
                LastName = input.LastName,
                Email = input.Email,
                UserName = input.UserName,
                Gender = input.Gender,
                DateOfBirth = input.DateOfBirth.HasValue ? DateOnly.FromDateTime(input.DateOfBirth.Value) : (DateOnly?)null,
                IsBlocked = false
            };

            var result = await _userManager.CreateAsync(appUser, input.Password);
            if (!result.Succeeded)
                throw new Exception(result.Errors.Select(x => x.Description).FirstOrDefault());

            var roleResult = await _userManager.AddToRoleAsync(appUser, "User");
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(appUser);
                throw new Exception("Failed to assign User role: " + string.Join(", ", roleResult.Errors.Select(e => e.Description)));
            }

            return new UserDto
            {
                Id = Guid.Parse(appUser.Id),
                FirstName = appUser.firstName,
                LastName = appUser.LastName,
                UserName = appUser.UserName,
                Email = appUser.Email,
                Gender = appUser.Gender,
                Age = appUser.DateOfBirth.HasValue ? CalculateAge(appUser.DateOfBirth) : null,
                Token = await _tokenService.GenerateJwtToken(appUser), 
                ProfilePicture = null
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
        public async Task<IEnumerable<UserSearchResultDto>> SearchUsersAsync(string term)
        {
            term = term?.ToLower() ?? "";

            var result = await _userManager.Users
                .Include(u => u.UserProfile)
                .Where(u =>
                    (!string.IsNullOrEmpty(u.UserName) && u.UserName.ToLower().Contains(term)) ||
                    (!string.IsNullOrEmpty(u.firstName) && u.firstName.ToLower().Contains(term)) ||
                    (!string.IsNullOrEmpty(u.LastName) && u.LastName.ToLower().Contains(term))
                )
                .Select(u => new UserSearchResultDto
                {
                    Id = u.Id,
                    UserName = $"{u.firstName} {u.LastName}".Trim(),
                    ProfilePicture = u.UserProfile != null ? u.UserProfile.ProfilePhoto : null
                })
                .ToListAsync();

            return result;
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

        public async Task<List<AdminUserDto>> GetAllUsersForAdminAsync()
        {
            var users = await _context.Users
                .Include(u => u.CommunityModerators)
                    .ThenInclude(cm => cm.Community)
                .Include(u => u.UserProfile)
                .ToListAsync();

            // جلب كل الـ Roles مرة واحدة
            var userIds = users.Select(u => u.Id).ToList();
            var userRoles = await _context.UserRoles
                .Where(ur => userIds.Contains(ur.UserId))
                .ToListAsync();

            var rolesDict = await _context.Roles.ToDictionaryAsync(r => r.Id, r => r.Name);

            var userDtos = new List<AdminUserDto>();
            foreach (var user in users)
            {
                // جلب أول Role للمستخدم (أو User لو مفيش)
                var roleId = userRoles.FirstOrDefault(ur => ur.UserId == user.Id)?.RoleId;
                var role = roleId != null && rolesDict.ContainsKey(roleId) ? rolesDict[roleId] : "User";

                var firstCommunity = user.CommunityModerators?.FirstOrDefault()?.Community;

                userDtos.Add(new AdminUserDto
                {
                    Id = user.Id,
                    Username = user.UserName,
                    Email = user.Email,
                    Role = role,
                    IsBlocked = user.IsBlocked,
                    CommunityId = user.CommunityModerators.Any() ? user.CommunityModerators.First().CommunityId : (long?)null,
                    ProfilePicture = user.UserProfile?.ProfilePhoto,
                    CommunityName = firstCommunity?.Name
                });
            }

            return userDtos;
        }




        private string GetPropertyValue(ApplicationUser user, string propertyName)
        {
            var property = user.GetType().GetProperty(propertyName);
            if (property == null)
                throw new Exception("Invalid property name.");

            return property.GetValue(user)?.ToString();
        }

        private static int? CalculateAge(DateOnly? dateOfBirth)
        {
            if (!dateOfBirth.HasValue) return null;

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var age = today.Year - dateOfBirth.Value.Year;
            if (dateOfBirth.Value > today.AddYears(-age)) age--;
            return age;
        }
    }
}