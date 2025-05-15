using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Service.ExchangeRequestService.DTOS
{
    public class ExchangeRequestDto
    {
        public long Id { get; set; }
        public string? SenderUserId { get; set; }
        public string? SenderUserName { get; set; }
        public string? SenderProfilePhoto { get; set; }
        public string? ReceiverUserId { get; set; }
        public string? ReceiverUserName { get; set; }
        public string? ReceiverProfilePhoto { get; set; }
        public string? BookTitle { get; set; }
        public string? AuthorName { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? RequestDate { get; set; }
        public bool? IsAccepted { get; set; }
        public string? longitude { get; set; }
        public string? latitude { get; set; }
    }
}
