/*
 * File: ExceptionHandlingMiddleware.cs
 * Project: Exception Handling Demo
 * Created: May 2024
 *
 * Description:
 * Centralized middleware for handling exceptions in ASP.NET Core applications.
 * Implements retry policies with exponential backoff and circuit breaker patterns
 * for system-level exceptions. Provides structured logging, metrics tracking,
 * and standardized error responses with correlation IDs.
 *
 * Copyright (c) 2024. All rights reserved.
 */

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
        private readonly MetricsService? _metricsService;

        // Cache the JSON serializer options
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger,
            MetricsService? metricsService = null)
        {
            _next = next;
            _logger = logger;
            _metricsService = metricsService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Capture the request context at the beginning
            var requestContext = RequestContext.FromHttpContext(context);

            try
            {
                await _next(context);
            }
            catch (SystemException systemEx) when (systemEx is not CircuitBreakerException)
            {
                // For system exceptions, apply retry with exponential backoff and circuit breaker
                // Use structured logging with correlation ID
                using (_logger.BeginScope(new { CorrelationId = requestContext.CorrelationId }))
                {
                    _logger.LogError(systemEx, "System exception occurred, applying retry policy");
                }

                try
                {
                    var retryService = context.RequestServices.GetRequiredService<RetryPolicyService>();

                    await retryService.ExecuteAsync(async (ct) =>
                    {
                        // Simulate the operation that would be retried
                        // In a real scenario, this would be the actual operation that failed
                        using (_logger.BeginScope(new { CorrelationId = requestContext.CorrelationId }))
                        {
                            _logger.LogInformation("Retrying operation...");
                        }

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

                    // Use structured logging with correlation ID
                    using (_logger.BeginScope(new { CorrelationId = requestContext.CorrelationId }))
                    {
                        _logger.LogError(circuitBreakerEx, "Circuit breaker is now open");
                    }

                    // Record the circuit breaker error in metrics
                    _metricsService?.RecordError("System Issue", 4500, requestContext.CorrelationId);

                    await HandleExceptionAsync(context, circuitBreakerEx, requestContext);
                    return;
                }
                catch (Exception retryEx)
                {
                    // All retries failed but circuit is not yet broken
                    using (_logger.BeginScope(new { CorrelationId = requestContext.CorrelationId }))
                    {
                        _logger.LogError(retryEx, "All retries failed");
                    }
                    await HandleExceptionAsync(context, systemEx, requestContext);
                }
            }
            catch (Exception ex)
            {
                // Handle all other exceptions normally
                using (_logger.BeginScope(new { CorrelationId = requestContext.CorrelationId }))
                {
                    _logger.LogError(ex, "An unhandled exception occurred");
                }
                await HandleExceptionAsync(context, ex, requestContext);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception, RequestContext requestContext)
        {
            context.Response.ContentType = "application/json";

            var errorResponse = new ErrorResponse
            {
                CorrelationId = requestContext.CorrelationId,
                Timestamp = DateTime.UtcNow,
                RequestContext = requestContext,
                DocumentationUrl = string.Empty,
                Suggestions = new System.Collections.Generic.List<string>()
            };

            int errorCode;
            string errorCategory;

            if (exception is BaseAppException appException)
            {
                // Handle our custom exceptions
                int statusCode = (int)appException.HttpStatusCode;
                context.Response.StatusCode = statusCode;
                errorResponse.HttpStatusCode = statusCode;
                errorResponse.ErrorCode = appException.ErrorCode;
                errorCode = appException.ErrorCode;
                errorResponse.ErrorCategory = ErrorResponse.GetErrorCategory(appException.ErrorCode);
                errorCategory = errorResponse.ErrorCategory;
                errorResponse.Message = appException.Message;
                errorResponse.DetailedMessage = GetDetailedMessage(appException);

                // Add documentation URL
                errorResponse.DocumentationUrl = ErrorResponse.GetDocumentationUrl(appException.ErrorCode);

                // Add suggestions
                errorResponse.Suggestions = ErrorResponse.GetSuggestions(appException.ErrorCode);
            }
            else
            {
                // Handle unexpected exceptions
                int statusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.StatusCode = statusCode;
                errorResponse.HttpStatusCode = statusCode;
                errorResponse.ErrorCode = 4000; // Default to system issue
                errorCode = 4000;
                errorResponse.ErrorCategory = ErrorResponse.GetErrorCategory(4000);
                errorCategory = errorResponse.ErrorCategory;
                errorResponse.Message = "An unexpected error occurred";
                errorResponse.DetailedMessage = GetDetailedMessage(exception);

                // Add documentation URL
                errorResponse.DocumentationUrl = ErrorResponse.GetDocumentationUrl(4000);

                // Add suggestions
                errorResponse.Suggestions = ErrorResponse.GetSuggestions(4000);

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

            // Record the error in metrics
            _metricsService?.RecordError(errorCategory, errorCode, requestContext.CorrelationId);

            // Log the error with structured data
            using (_logger.BeginScope(new
            {
                CorrelationId = requestContext.CorrelationId,
                ErrorCode = errorCode,
                ErrorCategory = errorCategory,
                Path = requestContext.Path,
                HttpMethod = requestContext.HttpMethod,
                ClientIp = requestContext.ClientIp,
                UserId = requestContext.UserId
            }))
            {
                _logger.LogError(
                    exception,
                    "Error handled: {ErrorCategory} {ErrorCode} - {Message}",
                    errorCategory,
                    errorCode,
                    errorResponse.Message);
            }

            var json = JsonSerializer.Serialize(errorResponse, JsonOptions);
            await context.Response.WriteAsync(json);
        }

        /// <summary>
        /// Creates a detailed error message from the exception
        /// </summary>
        private static string GetDetailedMessage(Exception exception)
        {
            var detailedMessage = exception.Message;

            // Add inner exception details if available
            if (exception.InnerException != null)
            {
                detailedMessage += $" Inner exception: {exception.InnerException.Message}";
            }

            // For custom exceptions, we might have additional properties to include
            if (exception is BaseAppException appException)
            {
                detailedMessage += $" (Error Code: {appException.ErrorCode})";
            }

            return detailedMessage;
        }
    }
}
