using AutoMapper;
using Microsoft.AspNetCore.Http;
using OnlineLibrary.Data.Entities;
using OnlineLibrary.Repository.Interfaces;
using OnlineLibrary.Repository.Specification;
using OnlineLibrary.Service.UserProfileService.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Service.UserProfileService
{
    public class UserProfiles : IUserProfile
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;


        public UserProfiles(
            IUnitOfWork unitOfWork,
            IMapper mapper)

        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;

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

            await _unitOfWork.Repository<UserProfile>().AddAsync(profile);
            await _unitOfWork.CountAsync();

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

            return _mapper.Map<UserProfileDto>(userProfile);
        }


        public Task UpdateCoverPhotoAsync(string userId, IFormFile file)
        {
            throw new NotImplementedException();
        }

        public async Task<UserProfileDto> UpdateProfileAsync(string userId, UserProfileUpdateDto profileDto)
        {
            {
                var profile = (await _unitOfWork.Repository<UserProfile>().GetAllWithSpecAsync(
                    new UserProfileWithUserSpecification(userId))).FirstOrDefault();

                if (profile == null)
                {
                    throw new Exception("Profile not found");
                }

                _mapper.Map(profileDto, profile);
                profile.LastUpdated = DateTime.UtcNow;

                _unitOfWork.Repository<UserProfile>().Update(profile);
                await _unitOfWork.CountAsync();

                return _mapper.Map<UserProfileDto>(profile);
            }
        }


        public Task UpdateProfilePhotoAsync(string userId, IFormFile file)
        {
            throw new NotImplementedException();
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
