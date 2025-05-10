using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Service.CommunityService.Dtos
{
    public class CreateCommentDto
    {
        public long PostId { get; set; }
        public string Content { get; set; }

        public IFormFile? ImageFile { get; set; }
    }
}
