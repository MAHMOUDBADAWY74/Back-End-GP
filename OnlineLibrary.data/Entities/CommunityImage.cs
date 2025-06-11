using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Data.Entities
{
    public class CommunityImage : BaseEntity
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public long CommunityId { get; set; }

        [Required]
        [StringLength(500)]
        public string ImageUrl { get; set; }

        [ForeignKey(nameof(CommunityId))]
        public Community Community { get; set; }
        public DateTime? UpdatedAt { get; set; } 

    }
}
