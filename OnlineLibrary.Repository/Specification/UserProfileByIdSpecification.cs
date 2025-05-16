using OnlineLibrary.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Repository.Specification
{
    public class UserProfileByIdSpecification : BaseSpecification<UserProfile>
    {
        public UserProfileByIdSpecification(long profileId)
            : base(p => p.Id == profileId)
        {
            Includes.Add(entity => entity.User);
        }
    }
}
