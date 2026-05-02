using AspNetCoreMvcDemoApp.Core.Data.Abstractions;
using AspNetCoreMvcDemoApp.Core.Data.Dapper;
using AspNetCoreMvcDemoApp.Core.Infrastructure.Caching;
using AspNetCoreMvcDemoApp.Core.Infrastructure.Caching.Abstractions;
using AspNetCoreMvcDemoApp.Core.Infrastructure.Workers;
using AspNetCoreMvcDemoApp.Core.Web.Common.Services;
using AspNetCoreMvcDemoApp.Core.Web.Handlers;
using AspNetCoreMvcDemoApp.Core.Web.ViewComponents.Menu;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace AspNetCoreMvcDemoApp.Core.Infrastructure.DependencyInjection
{
    public static class ServiceRegistrationExtensions
    {
        private static void AddAuthorization(this IServiceCollection services)
        {
            services.AddAuthorizationBuilder()
                .AddPolicy(nameof(RoleTypes.Admin), policy => policy.RequireClaim(ClaimTypes.Role, nameof(RoleTypes.Admin)))
                .AddPolicy(nameof(RoleTypes.Uploader), policy => policy.RequireClaim(ClaimTypes.Role, nameof(RoleTypes.Uploader)))
                .AddPolicy(nameof(RoleTypes.Downloader), policy => policy.RequireClaim(ClaimTypes.Role, nameof(RoleTypes.Downloader)));
        }

        private static void AddAuthentication(this IServiceCollection services)
        {
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Login/Index";
                    options.ExpireTimeSpan = TimeSpan.FromDays(7);
                    options.SlidingExpiration = true;
                    options.Cookie.Name = "DemoAppAuth";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SameSite = SameSiteMode.Lax;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                });
        }

        private static void AddDataService(this IServiceCollection services)
        {
            services.AddSingleton<IDataService, DataService>();
            DapperExtensions.AddCustomTypeHandlers();
        }

        private static void AddUserContextService(this IServiceCollection services)
        {
            services.AddScoped<IUserContext, UserContext>();
        }

        private static void AddMenuService(this IServiceCollection services)
        {
            services.AddScoped<IMenuService, MenuService>();
        }

        private static void AddExceptionHandlerService(this IServiceCollection services)
        {
            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();
        }

        private static void AddUserCacheService(this IServiceCollection services)
        {
            services.AddSingleton<IActiveUserCache, ActiveUserCache>();
        }

        private static void AddCommonServices(this IServiceCollection services)
        {
            services.AddScoped<ICommonServices, CommonServices>();
        }

        public static IServiceCollection AddApplicationInfrastructure(this IServiceCollection services)
        {
            // Register custom services in a logical order to ensure dependencies are resolved correctly.
            services.AddExceptionHandlerService();
            services.AddAuthorization();
            services.AddAuthentication();
            services.AddDataService();
            services.AddUserCacheService();
            services.AddUserContextService();
            services.AddMenuService();
            services.AddCommonServices();

            // Register the application initializer as a hosted service to run on startup.
            services.AddHostedService<ApplicationInitializer>();

            return services;
        }
    }
}
