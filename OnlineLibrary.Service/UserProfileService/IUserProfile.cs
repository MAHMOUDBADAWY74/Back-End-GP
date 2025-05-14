using Microsoft.AspNetCore.Http;
using OnlineLibrary.Service.UserProfileService.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Service.UserProfileService
{
    public interface IUserProfile
    {
        Task<UserProfileDto> GetProfileAsync(string userId);
        Task<UserProfileDto> CreateProfileAsync(string userId, UserProfileCreateDto profileDto);
        Task<UserProfileDto> UpdateProfileAsync(string userId, UserProfileUpdateDto profileDto);
        Task<UserProfileDto> GetProfileByIdAsync(long profileId);
    }
}
