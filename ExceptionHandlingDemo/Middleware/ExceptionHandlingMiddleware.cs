using System;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ExceptionHandlingDemo.Exceptions;
using ExceptionHandlingDemo.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using SystemException = ExceptionHandlingDemo.Exceptions.SystemException;

namespace ExceptionHandlingDemo.Middleware
{
    /// <summary>
    /// Middleware for handling exceptions in a centralized way
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        // Cache the JSON serializer options
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (SystemException systemEx) when (systemEx is not CircuitBreakerException)
            {
                // For system exceptions, apply retry with exponential backoff and circuit breaker
                _logger.LogError(systemEx, "System exception occurred, applying retry policy");

                try
                {
                    var retryService = context.RequestServices.GetRequiredService<RetryPolicyService>();

                    await retryService.ExecuteAsync(async (ct) =>
                    {
                        // Simulate the operation that would be retried
                        // In a real scenario, this would be the actual operation that failed
                        _logger.LogInformation("Retrying operation...");

                        // Add a small delay to make this truly async
                        await Task.Delay(10, ct);

                        // If the original exception is still thrown after all retries, it will be caught by the circuit breaker
                        throw systemEx;
                    }, CancellationToken.None);
                }
                catch (BrokenCircuitException circuitEx)
                {
                    // Circuit is now open after multiple failures
                    var circuitBreakerEx = new CircuitBreakerException(
                        "Service is currently unavailable due to too many failures. Please try again later.",
                        4500,
                        circuitEx);

                    _logger.LogError(circuitBreakerEx, "Circuit breaker is now open");
                    await HandleExceptionAsync(context, circuitBreakerEx);
                    return;
                }
                catch (Exception retryEx)
                {
                    // All retries failed but circuit is not yet broken
                    _logger.LogError(retryEx, "All retries failed");
                    await HandleExceptionAsync(context, systemEx);
                }
            }
            catch (Exception ex)
            {
                // Handle all other exceptions normally
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var errorResponse = new ErrorResponse
            {
                Timestamp = DateTime.UtcNow,
                Path = context.Request.Path
            };

            if (exception is BaseAppException appException)
            {
                // Handle our custom exceptions
                context.Response.StatusCode = (int)appException.HttpStatusCode;
                errorResponse.ErrorCode = appException.ErrorCode;
                errorResponse.ErrorCategory = ErrorResponse.GetErrorCategory(appException.ErrorCode);
                errorResponse.Message = appException.Message;
            }
            else
            {
                // Handle unexpected exceptions
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.ErrorCode = 4000; // Default to system issue
                errorResponse.ErrorCategory = ErrorResponse.GetErrorCategory(4000);
                errorResponse.Message = "An unexpected error occurred";

                // In development, we might want to include the actual exception message
                #if DEBUG
                errorResponse.Details = new
                {
                    ExceptionMessage = exception.Message,
                    ExceptionType = exception.GetType().Name,
                    StackTrace = exception.StackTrace
                };
                #endif
            }

            var json = JsonSerializer.Serialize(errorResponse, JsonOptions);
            await context.Response.WriteAsync(json);
        }
    }
}
