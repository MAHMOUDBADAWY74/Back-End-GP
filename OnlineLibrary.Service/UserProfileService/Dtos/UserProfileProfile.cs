using AutoMapper;
using OnlineLibrary.Data.Entities;
using OnlineLibrary.Service.CommunityService.Dtos;
using System;

namespace OnlineLibrary.Service.UserProfileService.Dtos
{
    public class UserProfileProfile : Profile
    {
        public UserProfileProfile()
        {
            CreateMap<UserProfile, UserProfileDto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.ProfilePhotoUrl, opt => opt.MapFrom(src => src.ProfilePhoto))
                .ForMember(dest => dest.CoverPhotoUrl, opt => opt.MapFrom(src => src.CoverPhoto))
                .ForMember(dest => dest.Bio, opt => opt.MapFrom(src => src.Bio))
                .ForMember(dest => dest.Hobbies, opt => opt.MapFrom(src => src.Hobbies != null ? src.Hobbies.ToArray() : null))
                .ForMember(dest => dest.FavoriteBookTopics, opt => opt.MapFrom(src => src.FavoriteBookTopics != null ? src.FavoriteBookTopics.ToArray() : null))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User != null ? src.User.firstName : null)) // Fixed to FirstName
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User != null ? src.User.LastName : null))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.User != null ? src.User.Gender : null))
                .ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.User != null && src.User.DateOfBirth.HasValue ? CalculateAge(src.User.DateOfBirth) : null))
                .ForMember(dest => dest.Posts, opt => opt.Ignore());

            CreateMap<UserProfileCreateDto, UserProfile>()
                .ForMember(dest => dest.Hobbies, opt => opt.MapFrom(src => src.Hobbies != null ? src.Hobbies.ToList() : null))
                .ForMember(dest => dest.FavoriteBookTopics, opt => opt.MapFrom(src => src.FavoriteBookTopics != null ? src.FavoriteBookTopics.ToList() : null));

            CreateMap<UserProfileUpdateDto, UserProfile>()
                .ForMember(dest => dest.Hobbies, opt => opt.MapFrom(src => src.Hobbies != null ? src.Hobbies.ToList() : null))
                .ForMember(dest => dest.FavoriteBookTopics, opt => opt.MapFrom(src => src.FavoriteBookTopics != null ? src.FavoriteBookTopics.ToList() : null));
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