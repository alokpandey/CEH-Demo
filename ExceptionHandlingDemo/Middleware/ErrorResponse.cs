using System;

namespace ExceptionHandlingDemo.Middleware
{
    /// <summary>
    /// Standard error response format
    /// </summary>
    public class ErrorResponse
    {
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
        /// Error message
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp of when the error occurred
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Request path that caused the error
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// Additional details about the error (optional)
        /// </summary>
        public object? Details { get; set; }

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
