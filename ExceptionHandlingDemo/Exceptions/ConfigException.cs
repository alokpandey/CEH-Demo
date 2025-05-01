using System;
using System.Net;

namespace ExceptionHandlingDemo.Exceptions
{
    /// <summary>
    /// Exception for configuration errors (code range: 1000-1999)
    /// </summary>
    public class ConfigException : BaseAppException
    {
        public ConfigException(string message, int errorCode, HttpStatusCode httpStatusCode = HttpStatusCode.InternalServerError) 
            : base(message, ValidateErrorCode(errorCode), httpStatusCode)
        {
        }

        public ConfigException(string message, int errorCode, Exception innerException, HttpStatusCode httpStatusCode = HttpStatusCode.InternalServerError) 
            : base(message, ValidateErrorCode(errorCode), innerException, httpStatusCode)
        {
        }

        private static int ValidateErrorCode(int errorCode)
        {
            if (errorCode < 1000 || errorCode > 1999)
            {
                throw new ArgumentOutOfRangeException(nameof(errorCode), 
                    "Config error codes must be between 1000 and 1999");
            }
            return errorCode;
        }
    }
}
