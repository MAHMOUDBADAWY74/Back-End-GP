using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Service.AdminService.Dtos
{
    public class CommunityPostSummaryDto
    {
        public string Id { get; set; }
        public string Content { get; set; }
        public long? CommunityId { get; set; }
        public string CreatedAt { get; set; }
        public string UserId { get; set; }
    }
}
