using OnlineLibrary.Service.CommunityService.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Service.UserProfileService.Dtos
{
    public class UserProfileDto
    {
        public string? UserId { get; set; }
        public string? ProfilePhotoUrl { get; set; }
        public string? CoverPhotoUrl { get; set; }
        public string? Bio { get; set; }
        public string[]? Hobbies { get; set; } 
        public string[]? FavoriteBookTopics { get; set; } 
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Gender { get; set; }
        public int? Age { get; set; }
        public List<CommunityPostDto>? Posts { get; set; } 
    }
}
