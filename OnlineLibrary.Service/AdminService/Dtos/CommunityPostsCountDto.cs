using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Service.AdminService.Dtos
{
    public class CommunityPostsCountDto
    {
        public string CommunityID { get; set; }
        public int PostCount { get; set; }
    }
}
