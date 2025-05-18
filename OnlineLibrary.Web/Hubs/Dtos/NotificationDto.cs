using System;

namespace OnlineLibrary.Web.Hubs.Dtos
{
    public class NotificationDto
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public string ProfilePicture { get; set; }
        public string Text { get; set; }
        public DateTime Time { get; set; }
    }
}