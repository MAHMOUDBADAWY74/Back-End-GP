using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Data.Entities
{
    public class ApplicationUser :IdentityUser

    {
        public String? firstName {  get; set; }

        public String? LastName { get; set; }
        public Address Address { get; set; }
        public DateOnly? DateOfBirth { get; set; }

        public String? Gender { get; set; }
        public bool IsBlocked { get; set; } 
        public virtual ICollection<CommunityModerator>? CommunityModerators { get; set; }
        public virtual UserProfile? UserProfile { get; set; } 


    }
}
