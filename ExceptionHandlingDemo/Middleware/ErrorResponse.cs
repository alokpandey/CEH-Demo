/*
 * File: ErrorResponse.cs
 * Project: Exception Handling Demo
 * Created: May 2024
 * Author: Alok Pandey
 *
 * Description:
 * Defines the standardized error response format for the application.
 * Includes error codes, categories, correlation IDs, documentation links,
 * and suggestions for resolving errors.
 */

using System;
using System.Collections.Generic;
using System.Net;

namespace ExceptionHandlingDemo.Middleware
{
    /// <summary>
    /// Standard error response format
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// Unique identifier for tracking this error across systems
        /// </summary>
        public string CorrelationId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Error code categorized by type:
        /// - Config Error: 1000-1999
        /// - Data Issues: 2000-2999
        /// - Logical Application Bug: 3000-3999
        /// - System Issues: 4000-4999
        /// </summary>
        public int ErrorCode { get; set; }

        /// <summary>
        /// Error category based on the error code
        /// </summary>
        public string ErrorCategory { get; set; } = string.Empty;

        /// <summary>
        /// HTTP status code returned to the client
        /// </summary>
        public int HttpStatusCode { get; set; }

        /// <summary>
        /// Brief error message for display purposes
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Detailed error message with more information about the error
        /// </summary>
        public string DetailedMessage { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp of when the error occurred
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Request context information
        /// </summary>
        public RequestContext? RequestContext { get; set; }

        /// <summary>
        /// Link to documentation for this error
        /// </summary>
        public string DocumentationUrl { get; set; } = string.Empty;

        /// <summary>
        /// Suggestions for resolving the error
        /// </summary>
        public List<string> Suggestions { get; set; } = new List<string>();

        /// <summary>
        /// Additional details about the error (optional)
        /// </summary>
        public object? Details { get; set; }

        /// <summary>
        /// Gets the documentation URL for a specific error code
        /// </summary>
        public static string GetDocumentationUrl(int errorCode)
        {
            // Base URL for error documentation
            string baseUrl = "https://docs.example.com/errors";

            // Get the category from the error code
            string category = GetErrorCategory(errorCode).Replace(" ", "-").ToLower();

            // Return the full URL
            return $"{baseUrl}/{category}/{errorCode}";
        }

        /// <summary>
        /// Gets suggestions for resolving an error based on its code
        /// </summary>
        public static List<string> GetSuggestions(int errorCode)
        {
            var suggestions = new List<string>();

            // Add general suggestion based on error category
            switch (errorCode)
            {
                case >= 1000 and <= 1999:
                    suggestions.Add("Check your application configuration files.");
                    suggestions.Add("Verify environment variables are set correctly.");
                    break;
                case >= 2000 and <= 2999:
                    suggestions.Add("Verify database connection settings.");
                    suggestions.Add("Check if the database server is running.");
                    suggestions.Add("Ensure you have the correct permissions.");
                    break;
                case >= 3000 and <= 3999:
                    suggestions.Add("Review the request parameters for validity.");
                    suggestions.Add("Check the API documentation for correct usage.");
                    break;
                case >= 4000 and <= 4999:
                    suggestions.Add("The system is experiencing issues. Please try again later.");
                    suggestions.Add("If the problem persists, contact support with the correlation ID.");
                    break;
                default:
                    suggestions.Add("Contact support with the correlation ID for assistance.");
                    break;
            }

            // Add specific suggestions for common error codes
            switch (errorCode)
            {
                case 1001: // Configuration file missing
                    suggestions.Add("Ensure the configuration file exists in the expected location.");
                    break;
                case 2001: // Database connection failed
                    suggestions.Add("Check your database credentials.");
                    suggestions.Add("Verify network connectivity to the database server.");
                    break;
                case 4500: // Circuit breaker open
                    suggestions.Add("The service is temporarily unavailable due to multiple failures.");
                    suggestions.Add("Wait a few minutes before retrying.");
                    break;
            }

            return suggestions;
        }

        /// <summary>
        /// Determines the error category based on the error code
        /// </summary>
        public static string GetErrorCategory(int errorCode)
        {
            return errorCode switch
            {
                >= 1000 and <= 1999 => "Configuration Error",
                >= 2000 and <= 2999 => "Data Issue",
                >= 3000 and <= 3999 => "Logical Application Bug",
                >= 4000 and <= 4999 => "System Issue",
                _ => "Unknown Error"
            };
        }
    }
}
