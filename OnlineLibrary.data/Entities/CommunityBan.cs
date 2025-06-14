using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Data.Entities
{
    [Table("CommunityBan")]
    public class CommunityBan : BaseEntity
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public long CommunityId { get; set; }
        public string BannedById { get; set; }
        public DateTime BannedAt { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual Community Community { get; set; }
    }
}
