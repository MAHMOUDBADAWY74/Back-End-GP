
using Microsoft.AspNetCore.Mvc;
using OnlineLibrary.Repository.Interfaces;
using OnlineLibrary.Repository.Repositories;
using OnlineLibrary.Service.AdminService;
using OnlineLibrary.Service.BookService;
using OnlineLibrary.Service.BookService.Dtos;
using OnlineLibrary.Service.CommunityService;
using OnlineLibrary.Service.CommunityService.Dtos;
using OnlineLibrary.Service.HandleResponse;
using OnlineLibrary.Service.TokenService;
using OnlineLibrary.Service.UserService;
using OnlineLibrary.Service.ExchangeRequestService.DTOS;
using OnlineLibrary.Service.ExchangeRequestService;

using System.ComponentModel;

namespace OnlineLibrary.Web.Extensions
{
    public static class ApplicationServiceExtension
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddAutoMapper(typeof(BookProfile));
            services.AddAutoMapper(typeof(CommunityProfile));
            services.AddAutoMapper(typeof(ExchangeBooksProfile));

          services.AddScoped<IBookService, BookService>();

            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<ICommunityService, CommunityService>();
            services.AddScoped<IExchangeBooks, ExchangeBooks>();
           




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


