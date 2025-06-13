using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Data.Contexts;
using OnlineLibrary.Data.Entities;
using OnlineLibrary.Repository;
using OnlineLibrary.Service.BookService.Dtos;

namespace OnlineLibrary.Web.Helper
{
    public class ApplySeeding
    {
        public static async Task ApplySeedingAsync(WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var loggerFactory = services.GetRequiredService<ILoggerFactory>();

                try
                {
                    var context = services.GetRequiredService<OnlineLibraryIdentityDbContext>();
                    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>(); 

                    await context.Database.MigrateAsync();
                    await OnlineLibraryContextSeed.SeedUserAsync(userManager, roleManager); 
                }
                catch (Exception ex)
                {
                    var logger = loggerFactory.CreateLogger<ApplySeeding>();
                    logger.LogError(ex, "An error occurred while seeding the database.");
                }
            }
        }
    }
}


