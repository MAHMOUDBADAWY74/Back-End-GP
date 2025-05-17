using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Data.Entities
{
    public class CommunityModerator
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        [Required]
        public long CommunityId { get; set; }
        public Community Community { get; set; }

        public DateTime JoinedAt { get; set; } // Optional: Track when the user became a moderator
    }
}
