using OnlineLibrary.Data.Entities;
using OnlineLibrary.Repository.Specification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Repository.Specifications
{
    public class ChatMessageSpecification : BaseSpecification<ChatMessage>
    {
        public ChatMessageSpecification(string senderId, string receiverId)
            : base(m => (m.SenderId == senderId && m.ReceiverId == receiverId) ||
                        (m.SenderId == receiverId && m.ReceiverId == senderId))
        {
            Includes.Add(m => m.Sender);
            Includes.Add(m => m.Receiver);
            ApplyOrderByDescending(m => m.CreatedAt); 
        }
    }
}