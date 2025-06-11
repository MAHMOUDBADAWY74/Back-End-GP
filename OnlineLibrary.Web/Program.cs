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
using OnlineLibrary.Service.CommunityService.Dtos;
using OnlineLibrary.Service.ContentModerationService;

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
            builder.Services.AddLogging(logging => logging.AddConsole());
            // Add services to the container
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
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notificationHub"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            builder.Services.AddMemoryCache();

            builder.Services.AddApplicationServices();
            builder.Services.AddScoped<IAdminService, AdminService>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddTransient<IDesignTimeDbContextFactory<OnlineLibraryIdentityDbContext>, OnlineLibraryIdentityDbContextFactory>();
            builder.Services.AddHttpContextAccessor();

            // تسجيل خدمة مراقبة المحتوى
            builder.Services.AddScoped<IContentModerationService, ContentModerationService>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins("http://localhost:5173")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            builder.Services.AddSignalR();

            // Register AutoMapper
            builder.Services.AddAutoMapper(typeof(CommunityProfile).Assembly);

            var app = builder.Build();

            // Database seeding
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<OnlineLibraryIdentityDbContext>();
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

                context.Database.EnsureCreated();
                await OnlineLibraryContextSeed.SeedUserAsync(userManager, roleManager);
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors("AllowFrontend");

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseStaticFiles();

            try
            {
                var imagesPath = Path.Combine(app.Environment.WebRootPath, "images");
                if (!Directory.Exists(imagesPath))
                {
                    Directory.CreateDirectory(imagesPath);
                    Console.WriteLine("Created wwwroot/images directory.");
                }

                var postImagesPath = Path.Combine(app.Environment.WebRootPath, "post-images");
                if (!Directory.Exists(postImagesPath))
                {
                    Directory.CreateDirectory(postImagesPath);
                    Console.WriteLine("Created wwwroot/post-images directory.");
                }
                var profilePhotosPath = Path.Combine(app.Environment.WebRootPath, "profile-photos");
                if (!Directory.Exists(profilePhotosPath))
                {
                    Directory.CreateDirectory(profilePhotosPath);
                    Console.WriteLine("Created wwwroot/profile-photos directory.");
                }

                var coverProfilePhotosPath = Path.Combine(app.Environment.WebRootPath, "cover-profile-photos");
                if (!Directory.Exists(coverProfilePhotosPath))
                {
                    Directory.CreateDirectory(coverProfilePhotosPath);
                    Console.WriteLine("Created wwwroot/cover-profile-photos directory.");
                }
                var communityimagesPath = Path.Combine(app.Environment.WebRootPath, "community-images");
                if (!Directory.Exists(communityimagesPath))
                {
                    Directory.CreateDirectory(communityimagesPath);
                    Console.WriteLine("Created wwwroot/community-images directory.");
                }
                // Test write access once
                var testFilePath = Path.Combine(imagesPath, "test.txt");
                if (!File.Exists(testFilePath))
                {
                    await File.WriteAllTextAsync(testFilePath, "Test write access");
                    Console.WriteLine("Write access to wwwroot/images is working.");
                    File.Delete(testFilePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to set up image directories: {ex.Message}");
            }

            app.MapHub<NotificationHub>("/notificationHub");
            app.MapHub<ChatHub>("/chatHub");

            app.MapControllers();

            await app.RunAsync();
        }
    }
}
