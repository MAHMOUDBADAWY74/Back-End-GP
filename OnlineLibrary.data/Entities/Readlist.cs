using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Data.Entities
{
    public class Readlist : BaseEntity
    {
        public long Id { get; set; }
        public string ?UserId { get; set; }
        public long? BookId { get; set; }
       


        public ApplicationUser? User { get; set; }
        public BooksDatum? Book { get; set; }
    }
}
