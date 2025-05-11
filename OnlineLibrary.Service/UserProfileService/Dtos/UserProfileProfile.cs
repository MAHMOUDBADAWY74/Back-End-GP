using AutoMapper;
using OnlineLibrary.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace OnlineLibrary.Service.UserProfileService.Dtos
{
    public class UserProfileProfile :Profile
    {
        public UserProfileProfile()
        {
            CreateMap<UserProfile, UserProfileDto>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User!.firstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User!.LastName))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.User!.Gender))
                .ForMember(dest => dest.Age, opt => opt.MapFrom(src => CalculateAge(src.User!.DateOfBirth)));

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
