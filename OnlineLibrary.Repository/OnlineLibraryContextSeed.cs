using Microsoft.AspNetCore.Identity;
using OnlineLibrary.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Repository
{
    public class OnlineLibraryContextSeed
    {
        public static async Task SeedUserAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            string[] roles = new[] { "Admin", "User", "Receiver", "Sender", "Moderator" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                    Console.WriteLine($"Role {role} created.");
                }
            }

            var users = new List<(string Email, string UserName, string FirstName, string LastName, string Role, string Password)>
            {
                ("admin@gmail.com", "Admin", "Admin", "User", "Admin", "Admin123!"),
                ("user@gmail.com", "RegularUser", "Regular", "User", "User", "User123!"),
                ("receiver@gmail.com", "Receiver", "Receiver", "User", "Receiver", "Receiver123!"),
                ("sender@gmail.com", "Sender", "Sender", "User", "Sender", "Sender123!"),
                ("moderator@gmail.com", "Moderator", "Moderator", "User", "Moderator", "Moderator123!")
            };

            foreach (var userData in users)
            {
                var existingUser = await userManager.FindByEmailAsync(userData.Email);
                if (existingUser == null)
                {
                    var user = new ApplicationUser
                    {
                        firstName = userData.FirstName,
                        LastName = userData.LastName,
                        Email = userData.Email,
                        UserName = userData.UserName,
                        Gender = "Unknown",
                        DateOfBirth = DateOnly.FromDateTime(DateTime.Now),
                        Address = new Address
                        {
                            FirstName = userData.FirstName,
                            City = "Cairo",
                            State = "Cairo",
                            Street = "Unknown",
                            PostalCode = "12345"
                        }
                    };

                    var result = await userManager.CreateAsync(user, userData.Password);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, userData.Role);
                        Console.WriteLine($"User {userData.Email} created and assigned to {userData.Role} role.");
                    }
                    else
                    {
                        Console.WriteLine($"Failed to create user {userData.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
                else
                {
                    Console.WriteLine($"User {userData.Email} already exists.");
                }
            }
        }
    }
}