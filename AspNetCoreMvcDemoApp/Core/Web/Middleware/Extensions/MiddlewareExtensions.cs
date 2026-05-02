namespace AspNetCoreMvcDemoApp.Core.Web.Middleware.Extensions
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomMiddlewares(this IApplicationBuilder app)
        {
            app.UseFileSecurity();

            return app;
        }
    }
}
