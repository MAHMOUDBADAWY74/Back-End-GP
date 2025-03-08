using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Data.Entities
{
    public class Wishlist : BaseEntity
    {
        public long Id { get; set; }
        public string? UserId { get; set; } 
        public long? BookId { get; set; }  
        public DateTime? AddedOn { get; set; } = DateTime.UtcNow;

      
        public ApplicationUser? User { get; set; }
        public BooksDatum? Book { get; set; }
    }
}
                    //The books in the plan are scheduled to be read 