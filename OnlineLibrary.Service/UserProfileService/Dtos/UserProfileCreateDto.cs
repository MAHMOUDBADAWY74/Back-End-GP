using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace OnlineLibrary.Service.UserProfileService.Dtos
{
    public class UserProfileCreateDto
    {
        public string? Bio { get; set; }
        public string[]? Hobbies { get; set; } 
        public string[]? FavoriteBookTopics { get; set; } 
        public IFormFile? ProfilePhoto { get; set; } 
        public IFormFile? CoverPhoto { get; set; }
        
    }
}