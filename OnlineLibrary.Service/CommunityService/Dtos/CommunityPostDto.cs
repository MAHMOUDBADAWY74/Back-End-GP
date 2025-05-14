using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Service.CommunityService.Dtos
{

    public class CommunityPostDto
    {
        public long Id { get; set; }
        public string Content { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public long? CommunityId { get; set; }
        public string CommunityName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ImageUrl { get; set; }
        public int LikeCount { get; set; }
        public bool IsLiked { get; set; }
        public int UnlikeCount { get; set; } 
        public bool IsUnliked { get; set; }  
        public int CommentCount { get; set; }
        public int ShareCount { get; set; }
    }
}
