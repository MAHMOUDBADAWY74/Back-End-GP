using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace OnlineLibrary.Service.UserProfileService.Dtos
{
    public class UserProfileUpdateDto
    {
        public string? Bio { get; set; }
        public IList<string>? Hobbies { get; set; }
        public IList<string>? FavoriteBookTopics { get; set; }

    }
}