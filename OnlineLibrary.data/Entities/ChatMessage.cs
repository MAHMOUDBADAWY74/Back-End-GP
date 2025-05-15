using System;
using System.ComponentModel.DataAnnotations;

namespace OnlineLibrary.Data.Entities
{
    public class ChatMessage : BaseEntity
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public string SenderId { get; set; }

        [Required]
        public string ReceiverId { get; set; }

        [Required]
        public string Message { get; set; }

        // public DateTime SentAt { get; set; } = DateTime.UtcNow; 

        public bool IsRead { get; set; } = false;

        public ApplicationUser Sender { get; set; }
        public ApplicationUser Receiver { get; set; }
    }
}