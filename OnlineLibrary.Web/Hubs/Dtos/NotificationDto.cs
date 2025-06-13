using System;

namespace OnlineLibrary.Web.Hubs.Dtos
{
    public class NotificationDto
    {
        public long Id { get; set; }
        public string NotificationType { get; set; }
        public string Message { get; set; }
        public string ActorUserId { get; set; }
        public string ActorUserName { get; set; }
        public string ActorProfilePicture { get; set; }
        public long? RelatedEntityId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string TimeAgo { get; set; }
    }
}