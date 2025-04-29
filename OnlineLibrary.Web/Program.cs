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
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                WebRootPath = "wwwroot"
            });

            // إضافة الخدمات
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

            // تسجيل DbContextFactory
            builder.Services.AddTransient<IDesignTimeDbContextFactory<OnlineLibraryIdentityDbContext>, OnlineLibraryIdentityDbContextFactory>();

            // إضافة CORS
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

            // تطبيق الـ Seeding
            await ApplySeeding.ApplySeedingAsync(app);

            // إعداد الـ Middleware
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // التأكد من وجود فولدرات الصور
            try
            {
                // فولدر images
                var imagesPath = Path.Combine(app.Environment.WebRootPath, "images");
                if (!Directory.Exists(imagesPath))
                {
                    Directory.CreateDirectory(imagesPath);
                    Console.WriteLine("Created wwwroot/images directory.");
                }

                // اختبار الكتابة في images
                var testFilePath = Path.Combine(imagesPath, "test.txt");
                await File.WriteAllTextAsync(testFilePath, "Test write access");
                Console.WriteLine("Write access to wwwroot/images is working.");
                File.Delete(testFilePath);

                // فولدر post-images
                var postImagesPath = Path.Combine(app.Environment.WebRootPath, "post-images");
                if (!Directory.Exists(postImagesPath))
                {
                    Directory.CreateDirectory(postImagesPath);
                    Console.WriteLine("Created wwwroot/post-images directory.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to set up image directories: {ex.Message}");
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCors("AllowAll");
            app.UseStaticFiles(); // لخدمة الملفات الثابتة مثل الصور
            app.MapControllers();

            app.Run();
        }
    }
}