using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Service.CommunityService.Dtos
{
    public class ModeratorDto
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string ProfilePicture { get; set; }
        public long CommunityId { get; set; }
        public string CommunityName { get; set; }
    }
}
