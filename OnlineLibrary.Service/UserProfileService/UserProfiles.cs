using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using OnlineLibrary.Data.Entities;
using OnlineLibrary.Repository.Interfaces;
using OnlineLibrary.Repository.Specification;
using OnlineLibrary.Service.UserProfileService.Dtos;
using System;
using System.IO;
using System.Threading.Tasks;

namespace OnlineLibrary.Service.UserProfileService
{
    public class UserProfiles : IUserProfile
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserProfiles(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<UserProfileDto> CreateProfileAsync(string userId, UserProfileCreateDto profileDto)
        {
            var existingProfile = await _unitOfWork.Repository<UserProfile>().GetAllWithSpecAsync(
                new UserProfileWithUserSpecification(userId));

            if (existingProfile.Any())
            {
                throw new Exception("Profile already exists");
            }

            var profile = _mapper.Map<UserProfile>(profileDto);
            profile.UserId = userId;

            // Handle Profile Photo upload
            if (profileDto.ProfilePhoto != null && profileDto.ProfilePhoto.Length > 0)
            {
                var sanitizedName = Guid.NewGuid().ToString();
                var fileName = $"{sanitizedName}{Path.GetExtension(profileDto.ProfilePhoto.FileName)}";
                var filePath = Path.Combine("wwwroot", "profile-photos", fileName);

                Directory.CreateDirectory(Path.Combine("wwwroot", "profile-photos"));
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profileDto.ProfilePhoto.CopyToAsync(stream);
                }
                profile.ProfilePhoto = $"/profile-photos/{fileName}";
            }

            // Handle Cover Photo upload
            if (profileDto.CoverPhoto != null && profileDto.CoverPhoto.Length > 0)
            {
                var sanitizedName = Guid.NewGuid().ToString();
                var fileName = $"{sanitizedName}{Path.GetExtension(profileDto.CoverPhoto.FileName)}";
                var filePath = Path.Combine("wwwroot", "cover-profile-photos", fileName);

                Directory.CreateDirectory(Path.Combine("wwwroot", "cover-profile-photos"));
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profileDto.CoverPhoto.CopyToAsync(stream);
                }
                profile.CoverPhoto = $"/cover-profile-photos/{fileName}";
            }

            await _unitOfWork.Repository<UserProfile>().AddAsync(profile);
            await _unitOfWork.CountAsync();

            // تحميل ApplicationUser قبل الـ mapping
            profile.User = await _userManager.FindByIdAsync(userId);

            return _mapper.Map<UserProfileDto>(profile);
        }

        public async Task<UserProfileDto> GetProfileAsync(string userId)
        {
            var profile = await _unitOfWork.Repository<UserProfile>().GetAllWithSpecAsync(
                new UserProfileWithUserSpecification(userId));

            var userProfile = profile.FirstOrDefault();
            if (userProfile == null)
            {
                var defaultProfile = new UserProfile { UserId = userId };
                await _unitOfWork.Repository<UserProfile>().AddAsync(defaultProfile);
                await _unitOfWork.CountAsync();
                userProfile = defaultProfile;
            }

            if (userProfile.User == null)
            {
                var user = await _userManager.FindByIdAsync(userId);
                userProfile.User = user;
            }

            return _mapper.Map<UserProfileDto>(userProfile);
        }

        public async Task<UserProfileDto> UpdateProfileAsync(string userId, UserProfileUpdateDto profileDto)
        {
            var profile = (await _unitOfWork.Repository<UserProfile>().GetAllWithSpecAsync(
                new UserProfileWithUserSpecification(userId))).FirstOrDefault();

            if (profile == null)
            {
                throw new Exception("Profile not found");
            }

            _mapper.Map(profileDto, profile);
            profile.LastUpdated = DateTime.UtcNow;

            // Handle Profile Photo upload (update or replace)
            if (profileDto.ProfilePhoto != null && profileDto.ProfilePhoto.Length > 0)
            {
                if (!string.IsNullOrEmpty(profile.ProfilePhoto))
                {
                    var oldFilePath = Path.Combine("wwwroot", profile.ProfilePhoto.TrimStart('/'));
                    if (File.Exists(oldFilePath)) File.Delete(oldFilePath);
                }

                var sanitizedName = Guid.NewGuid().ToString();
                var fileName = $"{sanitizedName}{Path.GetExtension(profileDto.ProfilePhoto.FileName)}";
                var filePath = Path.Combine("wwwroot", "profile-photos", fileName);

                Directory.CreateDirectory(Path.Combine("wwwroot", "profile-photos"));
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profileDto.ProfilePhoto.CopyToAsync(stream);
                }
                profile.ProfilePhoto = $"/profile-photos/{fileName}";
            }

            // Handle Cover Photo upload (update or replace)
            if (profileDto.CoverPhoto != null && profileDto.CoverPhoto.Length > 0)
            {
                if (!string.IsNullOrEmpty(profile.CoverPhoto))
                {
                    var oldFilePath = Path.Combine("wwwroot", profile.CoverPhoto.TrimStart('/'));
                    if (File.Exists(oldFilePath)) File.Delete(oldFilePath);
                }

                var sanitizedName = Guid.NewGuid().ToString();
                var fileName = $"{sanitizedName}{Path.GetExtension(profileDto.CoverPhoto.FileName)}";
                var filePath = Path.Combine("wwwroot", "cover-profile-photos", fileName);

                Directory.CreateDirectory(Path.Combine("wwwroot", "cover-profile-photos"));
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profileDto.CoverPhoto.CopyToAsync(stream);
                }
                profile.CoverPhoto = $"/cover-profile-photos/{fileName}";
            }

            _unitOfWork.Repository<UserProfile>().Update(profile);
            await _unitOfWork.CountAsync();

            // تحميل ApplicationUser قبل الـ mapping
            profile.User = await _userManager.FindByIdAsync(userId);

            return _mapper.Map<UserProfileDto>(profile);
        }

        public class UserProfileWithUserSpecification : BaseSpecification<UserProfile>
        {
            public UserProfileWithUserSpecification(string userId)
                : base(p => p.UserId == userId)
            {
                Includes.Add(entity => entity.User);
            }
        }
    }
}