using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Data.Entities
{
    public class Community : BaseEntity
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public string? AdminId { get; set; }
        public int? PostCount { get; set; } 

        public ApplicationUser? Admin { get; set; }
        public ICollection<CommunityMember>? Members { get; set; }

       
        public ICollection<CommunityPost>? Posts { get; set; }
        public ICollection<PostShare>? PostShares { get; set; }

    }
}
