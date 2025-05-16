using Microsoft.AspNetCore.Http;
using OnlineLibrary.Service.UserProfileService.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineLibrary.Service.UserProfileService
{
    public interface IUserProfile
    {
        Task<UserProfileDto> GetProfileAsync(string userId);
        Task<UserProfileDto> CreateProfileAsync(string userId, UserProfileCreateDto profileDto);
        Task<UserProfileDto> UpdateProfileAsync(string userId, UserProfileUpdateDto profileDto);
        Task<UserProfileDto> GetProfileByIdAsync(long profileId);
        Task<UserProfileDto> UpdateProfilePhotoAsync(string userId, IFormFile profilePhotoUpdate);
        Task<UserProfileDto> UpdateCoverPhotoAsync(string userId, IFormFile coverPhotoUpdate);
        Task<string> GetProfileOwnerIdAsync(long profileId);
    }
}