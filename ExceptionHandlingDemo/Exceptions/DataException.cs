using System;
using System.Net;

namespace ExceptionHandlingDemo.Exceptions
{
    /// <summary>
    /// Exception for data issues (code range: 2000-2999)
    /// </summary>
    public class DataException : BaseAppException
    {
        public DataException(string message, int errorCode, HttpStatusCode httpStatusCode = HttpStatusCode.InternalServerError) 
            : base(message, ValidateErrorCode(errorCode), httpStatusCode)
        {
        }

        public DataException(string message, int errorCode, Exception innerException, HttpStatusCode httpStatusCode = HttpStatusCode.InternalServerError) 
            : base(message, ValidateErrorCode(errorCode), innerException, httpStatusCode)
        {
        }

        private static int ValidateErrorCode(int errorCode)
        {
            if (errorCode < 2000 || errorCode > 2999)
            {
                throw new ArgumentOutOfRangeException(nameof(errorCode), 
                    "Data error codes must be between 2000 and 2999");
            }
            return errorCode;
        }
    }
}
