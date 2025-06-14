using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Data.Entities
{
    public class CommunityBan
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public long CommunityId { get; set; }
        public Community Community { get; set; }

        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public DateTime BannedAt { get; set; } = DateTime.UtcNow;
        public string? BannedById { get; set; } 
    }
}
