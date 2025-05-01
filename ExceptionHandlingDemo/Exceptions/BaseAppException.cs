using System;
using System.Net;

namespace ExceptionHandlingDemo.Exceptions
{
    /// <summary>
    /// Base exception class for all application exceptions
    /// </summary>
    public abstract class BaseAppException : Exception
    {
        /// <summary>
        /// Error code categorized by type:
        /// - Config Error: 1000-1999
        /// - Data Issues: 2000-2999
        /// - Logical Application Bug: 3000-3999
        /// - System Issues: 4000-4999
        /// </summary>
        public int ErrorCode { get; }
        
        /// <summary>
        /// HTTP status code to return to the client
        /// </summary>
        public HttpStatusCode HttpStatusCode { get; }

        protected BaseAppException(string message, int errorCode, HttpStatusCode httpStatusCode = HttpStatusCode.InternalServerError) 
            : base(message)
        {
            ErrorCode = errorCode;
            HttpStatusCode = httpStatusCode;
        }

        protected BaseAppException(string message, int errorCode, Exception innerException, HttpStatusCode httpStatusCode = HttpStatusCode.InternalServerError) 
            : base(message, innerException)
        {
            ErrorCode = errorCode;
            HttpStatusCode = httpStatusCode;
        }
    }
}
