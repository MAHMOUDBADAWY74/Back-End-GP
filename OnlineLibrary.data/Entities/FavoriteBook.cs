using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Data.Entities
{
    public class FavoriteBook : BaseEntity
    {
        public long? Id { get; set; }
        public string? UserId { get; set; }
        public long? BookId { get; set; }
        public DateTime? Added { get; set; } = DateTime.UtcNow;

      
        public ApplicationUser? User { get; set; }
        public BooksDatum? Book { get; set; }
    }
}
                            // for recommendation model