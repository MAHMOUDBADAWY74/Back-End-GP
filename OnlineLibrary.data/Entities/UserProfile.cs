using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Data.Entities
{
    public class UserProfile :BaseEntity
    {
        public long Id { get; set; }
        public string? UserId { get; set; }
        public string? ProfilePhoto { get; set; }
        public string? CoverPhoto { get; set; }
        public string? Bio { get; set; }
        public List<string> Hobbies { get; set; }
        public List<string> FavoriteBookTopics { get; set; } 
        public DateTime? LastUpdated { get; set; } = DateTime.UtcNow;

        public ApplicationUser? User { get; set; }
    }
}
