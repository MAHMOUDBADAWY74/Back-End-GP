using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using OnlineLibrary.Data.Entities;
using OnlineLibrary.Repository.Interfaces;
using OnlineLibrary.Repository.Specification;
using OnlineLibrary.Service.UserProfileService.Dtos;
using OnlineLibrary.Service.CommunityService;
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
        private readonly ICommunityService _communityService;

        public UserProfiles(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            UserManager<ApplicationUser> userManager,
            ICommunityService communityService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
            _communityService = communityService;
        }

        public async Task<UserProfileDto> CreateProfileAsync(string userId, UserProfileCreateDto profileDto)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId), "User ID cannot be null or empty.");

            var existingProfile = await _unitOfWork.Repository<UserProfile>().GetAllWithSpecAsync(
                new UserProfileWithUserSpecification(userId));

            if (existingProfile.Any())
            {
                throw new Exception("Profile already exists");
            }

            var profile = _mapper.Map<UserProfile>(profileDto);
            profile.UserId = userId;
            profile.Hobbies = profile.Hobbies ?? new List<string>();
            profile.FavoriteBookTopics = profile.FavoriteBookTopics ?? new List<string>();

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

            profile.User = await _userManager.FindByIdAsync(userId);

            return _mapper.Map<UserProfileDto>(profile);
        }

        public async Task<UserProfileDto> GetProfileAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId), "User ID cannot be null or empty.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new Exception($"User with ID {userId} not found.");
            }

            var profile = await _unitOfWork.Repository<UserProfile>().GetAllWithSpecAsync(
                new UserProfileWithUserSpecification(userId));

            var userProfile = profile.FirstOrDefault();
            if (userProfile == null)
            {
                var defaultProfile = new UserProfile
                {
                    UserId = userId,
                    Hobbies = new List<string>(),
                    FavoriteBookTopics = new List<string>()
                };
                await _unitOfWork.Repository<UserProfile>().AddAsync(defaultProfile);
                await _unitOfWork.CountAsync();
                userProfile = defaultProfile;
            }

            if (userProfile.User == null)
            {
                userProfile.User = user;
            }

            var profileDto = _mapper.Map<UserProfileDto>(userProfile);

            var posts = await _communityService.GetUserPostsAsync(userId);
            profileDto.Posts = posts.ToList();

            return profileDto;
        }

        public async Task<UserProfileDto> GetProfileByIdAsync(long profileId)
        {
            if (profileId <= 0)
                throw new ArgumentException("Profile ID must be a positive number.", nameof(profileId));

            var profile = await _unitOfWork.Repository<UserProfile>().GetAllWithSpecAsync(
                new UserProfileByIdSpecification(profileId));

            var userProfile = profile.FirstOrDefault();
            if (userProfile == null)
            {
                throw new Exception($"Profile with ID {profileId} not found.");
            }

            if (userProfile.User == null)
            {
                userProfile.User = await _userManager.FindByIdAsync(userProfile.UserId);
            }

            var profileDto = _mapper.Map<UserProfileDto>(userProfile);

            var posts = await _communityService.GetUserPostsAsync(userProfile.UserId);
            profileDto.Posts = posts.ToList();

            return profileDto;
        }

        public async Task<UserProfileDto> UpdateProfileAsync(string userId, UserProfileUpdateDto profileDto)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId), "User ID cannot be null or empty.");

            var profile = (await _unitOfWork.Repository<UserProfile>().GetAllWithSpecAsync(
                new UserProfileWithUserSpecification(userId))).FirstOrDefault();

            if (profile == null)
            {
                throw new Exception("Profile not found");
            }

            _mapper.Map(profileDto, profile);
            profile.LastUpdated = DateTime.UtcNow;
            profile.Hobbies = profile.Hobbies ?? new List<string>();
            profile.FavoriteBookTopics = profile.FavoriteBookTopics ?? new List<string>();

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

        public class UserProfileByIdSpecification : BaseSpecification<UserProfile>
        {
            public UserProfileByIdSpecification(long profileId)
                : base(p => p.Id == profileId)
            {
                Includes.Add(entity => entity.User);
            }
        }
    }
}