using System;
using System.Net;

namespace ExceptionHandlingDemo.Exceptions
{
    /// <summary>
    /// Exception for system issues (code range: 4000-4999)
    /// </summary>
    public class SystemException : BaseAppException
    {
        public SystemException(string message, int errorCode, HttpStatusCode httpStatusCode = HttpStatusCode.InternalServerError) 
            : base(message, ValidateErrorCode(errorCode), httpStatusCode)
        {
        }

        public SystemException(string message, int errorCode, Exception innerException, HttpStatusCode httpStatusCode = HttpStatusCode.InternalServerError) 
            : base(message, ValidateErrorCode(errorCode), innerException, httpStatusCode)
        {
        }

        private static int ValidateErrorCode(int errorCode)
        {
            if (errorCode < 4000 || errorCode > 4999)
            {
                throw new ArgumentOutOfRangeException(nameof(errorCode), 
                    "System error codes must be between 4000 and 4999");
            }
            return errorCode;
        }
    }
}
