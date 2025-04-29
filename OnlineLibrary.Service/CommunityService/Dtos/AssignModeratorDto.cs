using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Service.CommunityService.Dtos
{
    public class AssignModeratorDto
    {
        public long CommunityId { get; set; }
        public string UserId { get; set; }
    }
}
