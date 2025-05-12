using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OnlineLibrary.Data.Contexts;
using OnlineLibrary.Data.Entities;
using OnlineLibrary.Web.Helper;
using OnlineLibrary.Web.Extensions;
using OnlineLibrary.Service.TokenService;
using OnlineLibrary.Service.AdminService;
using OnlineLibrary.Repository;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Design;
using Store.Web.Extentions;
using OnlineLibrary.Web.Hubs;

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

            // Configure JWT Authentication
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                };
            });

            // تسجيل الخدمات
            builder.Services.AddApplicationServices();
            builder.Services.AddScoped<IAdminService, AdminService>();
            builder.Services.AddScoped<ITokenService, TokenService>();
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

            // إضافة SignalR
            builder.Services.AddSignalR(); 

            var app = builder.Build();

            // Seed Roles and Users using OnlineLibraryContextSeed
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<OnlineLibraryIdentityDbContext>();
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

                // Create the database if it doesn't exist
                context.Database.EnsureCreated();

                // Seed the database with roles and users
                await OnlineLibraryContextSeed.SeedUserAsync(userManager, roleManager);
            }

            // إعداد الـ Middleware
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // التأكد من وجود فولدرات الصور
            try
            {
                var imagesPath = Path.Combine(app.Environment.WebRootPath, "images");
                if (!Directory.Exists(imagesPath))
                {
                    Directory.CreateDirectory(imagesPath);
                    Console.WriteLine("Created wwwroot/images directory.");
                }

                var testFilePath = Path.Combine(imagesPath, "test.txt");
                await File.WriteAllTextAsync(testFilePath, "Test write access");
                Console.WriteLine("Write access to wwwroot/images is working.");
                File.Delete(testFilePath);

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
            app.UseStaticFiles();

            
            app.MapHub<NotificationHub>("/notificationHub");

            app.MapControllers();

            await app.RunAsync();
        }
    }
}