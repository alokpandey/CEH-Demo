/*
 * File: RequestContext.cs
 * Project: Exception Handling Demo
 * Created: May 2024
 *
 * Description:
 * Captures detailed information about HTTP requests for debugging and logging purposes.
 * Includes HTTP method, path, query string, client IP, user agent, and headers.
 * Provides correlation ID tracking across systems.
 *
 * Copyright (c) 2024. All rights reserved.
 */

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace ExceptionHandlingDemo.Middleware
{
    /// <summary>
    /// Captures context information about the current request
    /// </summary>
    public class RequestContext
    {
        /// <summary>
        /// Unique identifier for the request
        /// </summary>
        public string CorrelationId { get; set; } = string.Empty;

        /// <summary>
        /// The HTTP method used for the request (GET, POST, etc.)
        /// </summary>
        public string HttpMethod { get; set; } = string.Empty;

        /// <summary>
        /// The path of the request
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// The query string of the request
        /// </summary>
        public string QueryString { get; set; } = string.Empty;

        /// <summary>
        /// The IP address of the client
        /// </summary>
        public string ClientIp { get; set; } = string.Empty;

        /// <summary>
        /// The user agent of the client
        /// </summary>
        public string UserAgent { get; set; } = string.Empty;

        /// <summary>
        /// The user ID if authenticated
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Selected request headers
        /// </summary>
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// When the request started
        /// </summary>
        public DateTime RequestTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Creates a RequestContext from an HttpContext
        /// </summary>
        public static RequestContext FromHttpContext(HttpContext context)
        {
            var requestContext = new RequestContext
            {
                CorrelationId = context.TraceIdentifier,
                HttpMethod = context.Request.Method,
                Path = context.Request.Path.Value ?? string.Empty,
                QueryString = context.Request.QueryString.Value ?? string.Empty,
                ClientIp = context.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
                RequestTime = DateTime.UtcNow
            };

            // Add user ID if authenticated
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                requestContext.UserId = context.User.Identity.Name ?? string.Empty;
            }

            // Add selected headers
            if (context.Request.Headers.TryGetValue("User-Agent", out var userAgent))
            {
                requestContext.UserAgent = userAgent.ToString();
            }

            // Add other important headers
            var headersToCapture = new[] { "Accept", "Referer", "X-Forwarded-For", "X-Correlation-ID" };
            foreach (var header in headersToCapture)
            {
                if (context.Request.Headers.TryGetValue(header, out var value))
                {
                    requestContext.Headers[header] = value.ToString();
                }
            }

            // If there's a custom correlation ID header, use it
            if (requestContext.Headers.TryGetValue("X-Correlation-ID", out var correlationId) && !string.IsNullOrEmpty(correlationId))
            {
                requestContext.CorrelationId = correlationId;
            }

            return requestContext;
        }
    }
}
