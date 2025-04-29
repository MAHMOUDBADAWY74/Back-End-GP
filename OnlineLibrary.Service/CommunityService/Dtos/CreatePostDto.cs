using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Service.CommunityService.Dtos
{
    public class CreatePostDto
    {
        
        [Required]
        public long CommunityId { get; set; }
        [Required]
        public string Content { get; set; }
        public IFormFile? ImageFile { get; set; }

        
    }

}
