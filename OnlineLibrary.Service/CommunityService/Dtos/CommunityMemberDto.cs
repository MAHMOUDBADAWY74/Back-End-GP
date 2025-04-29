using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Service.CommunityService.Dtos
{
    public class CommunityMemberDto
    {
        public long Id { get; set; }
        public long CommunityId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public bool IsModerator { get; set; }
        public DateTime JoinedAt { get; set; }
    }
}
