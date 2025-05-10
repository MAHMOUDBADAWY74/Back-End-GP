using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Data.Entities
{
    public class CommunityPost : BaseEntity
    {
        public long Id { get; set; }
        public string Content { get; set; }
       
        public string? ImageUrl { get; set; }
       
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public long? CommunityId { get; set; }
        public Community? Community { get; set; }
        public int? PostCount { get; set; }
        public int? CommentCount { get; set; } 



        public ICollection<PostLike>? Likes { get; set; }

      
        public ICollection<PostComment>? Comments { get; set; }

       
        public ICollection<PostShare>? Shares { get; set; }
    }
}
