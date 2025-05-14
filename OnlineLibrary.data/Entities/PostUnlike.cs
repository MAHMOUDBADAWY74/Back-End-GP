using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Data.Entities
{
    public class PostUnlike : BaseEntity
    {
        public long Id { get; set; }
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }
        public long? PostId { get; set; }
        public CommunityPost? Post { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
