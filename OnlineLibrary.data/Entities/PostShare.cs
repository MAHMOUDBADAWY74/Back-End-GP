using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Data.Entities
{
    public class PostShare : BaseEntity
    {
        public long Id { get; set; }
       
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public long? PostId { get; set; }
        public CommunityPost? Post { get; set; }

        public long? SharedWithCommunityId { get; set; }
        public Community? SharedWithCommunity { get; set; }
    }
}
