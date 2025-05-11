using AutoMapper;
using OnlineLibrary.Data.Entities;
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
                .ForMember(dest => dest.Hobbies, opt => opt.MapFrom(src => src.Hobbies))
                .ForMember(dest => dest.FavoriteBookTopics, opt => opt.MapFrom(src => src.FavoriteBookTopics))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.firstName)) // جلب FirstName
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))   // جلب LastName
                .ForMember(dest => dest.Gender, opt => opt.Ignore())
                .ForMember(dest => dest.Age, opt => opt.Ignore());

            CreateMap<UserProfileCreateDto, UserProfile>();
            CreateMap<UserProfileUpdateDto, UserProfile>();
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