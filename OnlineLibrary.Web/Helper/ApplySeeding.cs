using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Data.Contexts;
using OnlineLibrary.Repository;
using Microsoft.Extensions.Logging;
using OnlineLibrary.Data.Entities;

namespace OnlineLibrary.Web.Helper
{
    public class ApplySeeding
    {
        public static async Task ApplySeedingAsync(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            var loggerFactory = services.GetRequiredService<ILoggerFactory>();

            try
            {
                var context = services.GetRequiredService<OnlineLibraryIdentityDbContext>();
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

                // Apply migrations
                await context.Database.MigrateAsync();

                // Seed user data
                await OnlineLibraryContextSeed.SeedUserAsync(userManager);

                var logger = loggerFactory.CreateLogger<ApplySeeding>();
                logger.LogInformation("Database seeding completed successfully.");
            }
            catch (Exception ex)
            {
                var logger = loggerFactory.CreateLogger<ApplySeeding>();
                logger.LogError(ex, "An error occurred while seeding the database.");
                throw;
            }
        }
    }
}