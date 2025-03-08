using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Data.Entities
{
    public class BaseEntity
    {

       
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
