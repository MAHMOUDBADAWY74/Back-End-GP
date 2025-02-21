using Microsoft.AspNetCore.Mvc;
using OnlineLibrary.Service.AdminService;
using OnlineLibrary.Service.HandleResponse;
using OnlineLibrary.Service.TokenService;
using OnlineLibrary.Service.UserService;

namespace OnlineLibrary.Web.Extensions
{
    public  static class ApplicationServiceExtension
    {

#pragma warning disable IDE0060 // Remove unused parameter
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, KeyValuePair<string, Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateEntry?> model)
#pragma warning restore IDE0060 // Remove unused parameter
        {
          
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAdminService, AdminService>();
            _ = services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = actionContext =>
                {
                    // Collect all validation errors
                    var errors = actionContext.ModelState
                        .Where(model => model.Value?.Errors?.Count > 0)  // Use null-conditional operator
                        .SelectMany(model => model.Value!.Errors)         // Use null-forgiving operator (if sure)
                        .Select(error => error.ErrorMessage)
                        .ToList();

                    // Log validation errors
                    var logger = actionContext.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogError("Validation errors occurred: {@Errors}", errors);

                    // Create a validation error response
                    var errorRespone = new ValidationErrorResponse { Errors = errors };

                    // Return a 400 Bad Request with the error response
                    return new BadRequestObjectResult(errorRespone);
                };
            });
            return services;

        }
    }
}
