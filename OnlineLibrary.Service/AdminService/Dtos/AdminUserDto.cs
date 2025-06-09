using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Service.AdminService.Dtos
{
    public class AdminUserDto
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public bool IsBlocked { get; set; }
        public long? CommunityId { get; set; }
        public string ProfilePicture { get; set; }
        public string CommunityName { get; set; }
        
    }
}
