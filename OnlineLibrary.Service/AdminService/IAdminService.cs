using OnlineLibrary.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Service.AdminService
{
    public interface IAdminService
    {
       
        Task<List<PendingUserChange>> GetPendingChanges();
        Task<bool> ApproveChange(Guid changeId);
        Task<bool> RejectChange(Guid changeId);
    }
}
