using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Data.Entities
{
    public class Visit : BaseEntity
    {
        public long Id { get; set; }
        public DateTime VisitDate { get; set; }
        public string UserId { get; set; } // Optional: If you want to track which user visited
    }
}
