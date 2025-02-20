using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Data.Contexts;
using OnlineLibrary.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Service.AdminService
{
    public class AdminService : IAdminService
    {
        private readonly OnlineLibraryIdentityDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminService(OnlineLibraryIdentityDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<List<PendingUserChange>> GetPendingChanges()
        {
            return await _context.PendingUserChanges
                .Where(p => !p.IsApproved)
                .ToListAsync();
        }

        public async Task<bool> ApproveChange(Guid changeId)
        {
            var change = await _context.PendingUserChanges.FindAsync(changeId);
            if (change == null)
                throw new Exception("Change not found.");

            var user = await _userManager.FindByIdAsync(change.UserId);
            if (user == null)
                throw new Exception("User not found.");

            if (change.FieldName == "Delete")
            {
                await _userManager.DeleteAsync(user);
            }
            else
            {
                var property = user.GetType().GetProperty(change.FieldName);
                if (property == null)
                    throw new Exception("Invalid property name.");

                property.SetValue(user, change.NewValue);
                await _userManager.UpdateAsync(user);
            }

            change.IsApproved = true;
            change.ApprovedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RejectChange(Guid changeId)
        {
            var change = await _context.PendingUserChanges.FindAsync(changeId);
            if (change == null)
                throw new Exception("Change not found.");

            _context.PendingUserChanges.Remove(change);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
