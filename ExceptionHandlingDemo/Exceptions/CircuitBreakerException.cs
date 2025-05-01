using System;
using System.Net;

namespace ExceptionHandlingDemo.Exceptions
{
    /// <summary>
    /// Exception thrown when the circuit breaker is open
    /// </summary>
    public class CircuitBreakerException : SystemException
    {
        public CircuitBreakerException(string message, int errorCode = 4500) 
            : base(message, errorCode, HttpStatusCode.ServiceUnavailable)
        {
        }

        public CircuitBreakerException(string message, int errorCode, Exception innerException) 
            : base(message, errorCode, innerException, HttpStatusCode.ServiceUnavailable)
        {
        }
    }
}
