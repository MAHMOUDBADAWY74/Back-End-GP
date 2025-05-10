using OnlineLibrary.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Service.CommunityService.Dtos
{
    public class PostCommentDto
    {
        public long Id { get; set; }
        public long? PostId { get; set; } // Updated to nullable to match PostComment
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Content { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
