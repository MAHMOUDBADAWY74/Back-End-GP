using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Data.Entities
{

    public class Notification : BaseEntity
    {
        public long Id { get; set; }
        public string? Message { get; set; }
        public bool? IsRead { get; set; } = false;

        public string? NotificationType { get; set; } 

        public string? UserId { get; set; } 
        public ApplicationUser? User { get; set; }

        public string? ActorUserId { get; set; } 
        public string? ActorUserName { get; set; }
        public string? ActorProfilePicture { get; set; }

        public long? RelatedEntityId { get; set; } 
    }


}
