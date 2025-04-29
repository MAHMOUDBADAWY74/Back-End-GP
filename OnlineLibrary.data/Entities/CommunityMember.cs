using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Data.Entities
{
    public class CommunityMember : BaseEntity
    {
        public long Id { get; set; }
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public long? CommunityId { get; set; }
        public Community? Community { get; set; }

        public bool? IsModerator { get; set; }

        public DateTime? JoinedDate { get; set; } = DateTime.UtcNow;
    }

   
}
