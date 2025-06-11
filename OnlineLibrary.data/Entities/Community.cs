using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Data.Entities
{
    public class Community : BaseEntity
    {

        public long Id { get; set; }
        [Required]
        [StringLength(450)]
        public string Name { get; set; }
        public string Description { get; set; }

        public string? AdminId { get; set; }
        public int? PostCount { get; set; } 

        public ApplicationUser? Admin { get; set; }
        public ICollection<CommunityMember>? Members { get; set; }

       
        public ICollection<CommunityPost>? Posts { get; set; }
        public ICollection<PostShare>? PostShares { get; set; }
        public virtual ICollection<CommunityImage>? Images { get; set; }

        public virtual ICollection<CommunityModerator>? Moderators { get; set; }

    }
}
