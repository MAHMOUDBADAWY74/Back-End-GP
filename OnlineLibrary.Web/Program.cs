using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using OnlineLibrary.Data.Contexts;
using OnlineLibrary.Data.Entities;
using OnlineLibrary.Web.Extensions;
using OnlineLibrary.Web.Helper;
using Store.Web.Extentions;

namespace OnlineLibrary.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerDocumentation();

            builder.Services.AddDbContext<OnlineLibraryIdentityDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<OnlineLibraryIdentityDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.AddApplicationServices();
            builder.Services.AddIdentityServices(builder.Configuration);

            // Register the DbContextFactory
            builder.Services.AddTransient<IDesignTimeDbContextFactory<OnlineLibraryIdentityDbContext>, OnlineLibraryIdentityDbContextFactory>();

            // Add CORS policy
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

            var app = builder.Build();

            // Apply seeding
            await ApplySeeding.ApplySeedingAsync(app);

            // Configure middleware pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            try
            {
                var testFilePath = Path.Combine("wwwroot/images", "test.txt");
                await File.WriteAllTextAsync(testFilePath, "Test write access");
                Console.WriteLine("Write access to wwwroot/images is working.");
                File.Delete(testFilePath); // حذف الملف بعد الاختبار
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Write access to wwwroot/images failed: {ex.Message}");
            }


            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();

            // Use CORS policy
            app.UseCors("AllowAll");

            app.MapControllers();

            app.UseStaticFiles();

            app.Run();
        }
    }
}



