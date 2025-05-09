using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Data.Contexts;
using OnlineLibrary.Data.Entities;
using OnlineLibrary.Service.AdminService;
using OnlineLibrary.Service.AdminService.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineLibrary.Service.AdminService
{
    public class AdminService : IAdminService
    {
        private readonly OnlineLibraryIdentityDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminService(
            OnlineLibraryIdentityDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<List<PendingUserChange>> GetPendingChanges()
        {
            return await _context.PendingUserChanges
                .Where(c => !c.IsApproved)
                .ToListAsync();
        }

        public async Task<bool> ApproveChange(Guid changeId)
        {
            var change = await _context.PendingUserChanges
                .FirstOrDefaultAsync(c => c.Id == changeId);

            if (change == null)
                throw new Exception("Change request not found.");

            change.IsApproved = true;
            change.ApprovedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectChange(Guid changeId)
        {
            var change = await _context.PendingUserChanges
                .FirstOrDefaultAsync(c => c.Id == changeId);

            if (change == null)
                throw new Exception("Change request not found.");

            _context.PendingUserChanges.Remove(change);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ChangeUserRole(string userId, ChangeRoleDto input)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found.");

            var roleExists = await _roleManager.RoleExistsAsync(input.NewRole);
            if (!roleExists)
                throw new Exception($"Role {input.NewRole} does not exist.");

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Contains(input.NewRole))
                return true;

            if (currentRoles.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                    throw new Exception("Failed to remove existing roles.");
            }

            var addResult = await _userManager.AddToRoleAsync(user, input.NewRole);
            if (!addResult.Succeeded)
                throw new Exception("Failed to add new role.");

            return true;
        }
    }
}