using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Service.ExchangeRequestService.DTOS
{
    public class CreateExchangeRequestDto
    {
        public string? BookTitle { get; set; }
        public string? AuthorName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? longitude { get; set; }
        public string? latitude { get; set; }
    }
}
