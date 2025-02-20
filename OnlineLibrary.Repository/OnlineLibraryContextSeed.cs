using Microsoft.AspNetCore.Identity;
using OnlineLibrary.Data.Entities;
using System.Threading.Tasks;

namespace OnlineLibrary.Repository
{
    public class OnlineLibraryContextSeed
    {
        public static async Task SeedUserAsync(UserManager<ApplicationUser> userManager)
        {
            // Check if there are no users in the database
            if (!userManager.Users.Any())
            {
                var user = new ApplicationUser
                {
                    firstName = "Shimaa",
                    LastName = "Nabil",
                    Email = "Shimaa@gmail.com",
                    Gender = "F",
                    DateOfBirth = DateOnly.Parse("2024-11-23"),
                    Address = new Address
                    {
                        FirstName = "Shimaa",
                        City = "Maadi",
                        State = "Cairo",
                        Street = "105",
                        PostalCode = "123456"
                    }
                };

                // Create the user with a password
                var result = await userManager.CreateAsync(user, "Password123!");

                if (result.Succeeded)
                {
                    Console.WriteLine("User created successfully!");
                }
                else
                {
                    Console.WriteLine("Failed to create user.");
                }
            }
            else
            {
                Console.WriteLine("Users already exist in the database.");
            }
        }
    }
}