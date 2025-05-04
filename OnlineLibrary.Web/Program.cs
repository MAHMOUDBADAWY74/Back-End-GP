using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Data.Contexts;
using OnlineLibrary.Data.Entities;
using OnlineLibrary.Web.Helper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using OnlineLibrary.Service.TokenService;
using Microsoft.EntityFrameworkCore.Design;
using Store.Web.Extentions;
using OnlineLibrary.Web.Extensions; // أضفنا الـ namespace لـ AddApplicationServices

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
            builder.Services.AddSwaggerDocumentation(); // من Store.Web.Extentions

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
            builder.Services.AddApplicationServices(); // دلوقتي هيشتغل مع OnlineLibrary.Web.Extensions
            // أزلنا AddIdentityServices لأنها مش موجودة

            // تسجيل TokenService (للتأكد)
            builder.Services.AddScoped<ITokenService, TokenService>();

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

            // Seed Admin User and Role
            using (var scope = app.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                // Ensure Admin role exists
                if (!await roleManager.RoleExistsAsync("Admin"))
                {
                    await roleManager.CreateAsync(new IdentityRole("Admin"));
                }

                // Ensure Admin user exists
                var adminUser = await userManager.FindByEmailAsync("admin@gmail.com");
                if (adminUser == null)
                {
                    adminUser = new ApplicationUser
                    {
                        Id = "f7420afa-2aeb-4026-9271-cf2549b215cd",
                        UserName = "Admin",
                        Email = "admin@gmail.com",
                        EmailConfirmed = true
                    };
                    var result = await userManager.CreateAsync(adminUser, "Admin@123");
                    if (!result.Succeeded)
                    {
                        throw new Exception("Failed to create admin user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }

                // Ensure Admin user has Admin role
                if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

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

            await app.RunAsync();
        }
    }
}