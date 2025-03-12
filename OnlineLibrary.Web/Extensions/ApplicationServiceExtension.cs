
using Microsoft.AspNetCore.Mvc;
using OnlineLibrary.Repository.Interfaces;
using OnlineLibrary.Repository.Repositories;
using OnlineLibrary.Service.AdminService;
using OnlineLibrary.Service.BookService;
using OnlineLibrary.Service.BookService.Dtos;
using OnlineLibrary.Service.HandleResponse;
using OnlineLibrary.Service.TokenService;
using OnlineLibrary.Service.UserService;

namespace OnlineLibrary.Web.Extensions
{
    public static class ApplicationServiceExtension
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // إضافة الخدمات
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddAutoMapper(typeof(BookProfile));
            services.AddScoped<IBookService, BookService>();

            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAdminService, AdminService>();

            // تخصيص إعدادات معالجة الأخطاء في API
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = actionContext =>
                {
                    var errors = actionContext.ModelState
                                .Where(model => model.Value?.Errors.Count() > 0)
                                .SelectMany(model => model.Value.Errors)
                                .Select(error => error.ErrorMessage).ToList();

                    var errorRespone = new ValidationErrorResopnse { Errors = errors };

                    return new BadRequestObjectResult(errorRespone);
                };
            });

            return services;
        }
    }
}


