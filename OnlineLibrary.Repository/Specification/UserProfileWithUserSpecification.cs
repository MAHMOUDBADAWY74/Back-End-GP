using OnlineLibrary.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Repository.Specification
{
    public class UserProfileWithUserSpecification : BaseSpecification<UserProfile>
    {
        public UserProfileWithUserSpecification(string userId)
            : base(p => p.UserId == userId)
        {
            Includes.Add(entity => entity.User);
        }
    }
}
