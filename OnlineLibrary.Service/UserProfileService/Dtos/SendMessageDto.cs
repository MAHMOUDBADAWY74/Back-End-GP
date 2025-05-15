using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Service.UserProfileService.Dtos
{
    public class SendMessageDto
    {
        public string ReceiverId { get; set; }
        public string Message { get; set; }
    }
}
