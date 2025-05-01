using Microsoft.AspNetCore.Builder;

namespace ExceptionHandlingDemo.Middleware
{
    /// <summary>
    /// Extension methods for registering the exception handling middleware
    /// </summary>
    public static class ExceptionHandlingMiddlewareExtensions
    {
        /// <summary>
        /// Adds the exception handling middleware to the application pipeline
        /// </summary>
        public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}
