using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AspNetCoreMvcDemoApp.Core.Web.Handlers
{
    using Core.Common.Exceptions;
    public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            logger.LogError(exception, "An unhandled exception occurred.");

            int statusCode = exception switch
            {
                UserNotFoundException => StatusCodes.Status401Unauthorized,
                UserNotAllowedException => StatusCodes.Status403Forbidden,
                NotImplementedException => StatusCodes.Status501NotImplemented,
                _ => StatusCodes.Status500InternalServerError
            };

            var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

            var problemDetails = new ProblemDetails
            {
                Title = "An error occurred",
                Status = statusCode,
                Detail = exception.Message,
                Type = exception.GetType().Name,
                Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}",
                Extensions = { ["traceId"] = traceId }
            };

            httpContext.Response.StatusCode = statusCode;

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }

}
