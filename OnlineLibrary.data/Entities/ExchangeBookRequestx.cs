using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Data.Entities
{
    public class ExchangeBookRequestx: BaseEntity
    {

         
        public long Id { get; set; }
        public string? SenderUserId { get; set; }
        public ApplicationUser? Sender { get; set; }
        public string? BookTitle { get; set; }
        public string? AuthorName { get; set; }
        public string? PhoneNumber { get; set; }

        public bool? IsAccepted { get; set; } = false;
        public string? ReceiverUserId { get; set; }
        public ApplicationUser? Receiver { get; set; }

        public string? longitude { get; set; }
        public string? latitude { get; set; }


    }
}

