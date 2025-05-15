using OnlineLibrary.Service.UserService.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Service.UserProfileService.Dtos
{
    public class ChatMessageDto
    {
        public long Id { get; set; }
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public ChatUserDto Sender { get; set; } 
        public ChatUserDto Receiver { get; set; } 
    }
}
