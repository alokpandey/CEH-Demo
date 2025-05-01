using System;
using System.Net;

namespace ExceptionHandlingDemo.Exceptions
{
    /// <summary>
    /// Exception for logical application bugs (code range: 3000-3999)
    /// </summary>
    public class LogicalException : BaseAppException
    {
        public LogicalException(string message, int errorCode, HttpStatusCode httpStatusCode = HttpStatusCode.InternalServerError) 
            : base(message, ValidateErrorCode(errorCode), httpStatusCode)
        {
        }

        public LogicalException(string message, int errorCode, Exception innerException, HttpStatusCode httpStatusCode = HttpStatusCode.InternalServerError) 
            : base(message, ValidateErrorCode(errorCode), innerException, httpStatusCode)
        {
        }

        private static int ValidateErrorCode(int errorCode)
        {
            if (errorCode < 3000 || errorCode > 3999)
            {
                throw new ArgumentOutOfRangeException(nameof(errorCode), 
                    "Logical error codes must be between 3000 and 3999");
            }
            return errorCode;
        }
    }
}
