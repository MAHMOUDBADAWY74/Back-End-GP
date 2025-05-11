using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Service.UserProfileService.Dtos
{
    public class UserProfileUpdateDto
    {
         
        public string? Bio { get; set; }
        public string? Hobbies { get; set; }
        public string? FavoriteBookTopics { get; set; }
    }
}

