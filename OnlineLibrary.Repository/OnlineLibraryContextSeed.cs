using Microsoft.AspNetCore.Identity;
using OnlineLibrary.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineLibrary.Repository
{
    public class OnlineLibraryContextSeed
    {
        public static async Task SeedUserAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Seed Roles
            string[] roles = new[] { "Admin", "User", "Receiver", "Sender", "Moderator" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var result = await roleManager.CreateAsync(new IdentityRole(role));
                    if (result.Succeeded)
                    {
                        Console.WriteLine($"Role {role} created.");
                    }
                    else
                    {
                        Console.WriteLine($"Failed to create role {role}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
            }

            // Seed Users
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
                        },
                        EmailConfirmed = true // To avoid email confirmation for seeded users
                    };

                    var result = await userManager.CreateAsync(user, userData.Password);
                    if (result.Succeeded)
                    {
                        var roleResult = await userManager.AddToRoleAsync(user, userData.Role);
                        if (roleResult.Succeeded)
                        {
                            Console.WriteLine($"User {userData.Email} created and assigned to {userData.Role} role.");
                        }
                        else
                        {
                            Console.WriteLine($"Failed to assign role {userData.Role} to user {userData.Email}: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Failed to create user {userData.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
                else
                {
                    // Check if the existing user has the correct role
                    var userRoles = await userManager.GetRolesAsync(existingUser);
                    if (!userRoles.Contains(userData.Role))
                    {
                        var roleResult = await userManager.AddToRoleAsync(existingUser, userData.Role);
                        if (roleResult.Succeeded)
                        {
                            Console.WriteLine($"Assigned role {userData.Role} to existing user {userData.Email}.");
                        }
                        else
                        {
                            Console.WriteLine($"Failed to assign role {userData.Role} to existing user {userData.Email}: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                        }
                    }
                    Console.WriteLine($"User {userData.Email} already exists.");
                }
            }
        }
    }
}