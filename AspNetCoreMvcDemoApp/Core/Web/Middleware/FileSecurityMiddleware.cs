namespace AspNetCoreMvcDemoApp.Core.Web.Middleware
{
    using AspNetCoreMvcDemoApp.Core.Web.Authentication.Abstractions;
    using Core.Common.Exceptions;

    public static class FileSecurityMiddleware
    {
        private static readonly PathString _securePath = new("/uploads/files");
        private static readonly RoleTypes[] _allowedRoles = { RoleTypes.Admin, RoleTypes.Uploader, RoleTypes.Downloader };

        public static IApplicationBuilder UseFileSecurity(this IApplicationBuilder app)
        {
            return app.Use(async (context, next) =>
            {
                if (context.Request.Path.StartsWithSegments(_securePath, StringComparison.OrdinalIgnoreCase))
                {
                    var userContext = context.RequestServices.GetRequiredService<IUserContext>();

                    userContext.CheckAuthorization(context.User);

                    if (!userContext.IsAuthorized)
                    {
                        throw new UserNotFoundException();
                    }

                    if (!_allowedRoles.Contains(userContext.Role))
                    {
                        throw new UserNotAllowedException();
                    }
                }

                await next();
            });
        }
    }
}
