using OnlineLibrary.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Service.CommunityService.Dtos
{
    public class CommunityDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
       
        public DateTime CreatedAt { get; set; }
       
        public int MemberCount { get; set; }
        public int PostCount { get; set; }
        public bool IsMember { get; set; }

        public string AdminId { get; set; }
        public string AdminName { get; set; }
        public string? ImageUrl { get; set; }

    }
}
