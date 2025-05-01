using System;
using System.Net;
using System.Threading.Tasks;
using ExceptionHandlingDemo.Exceptions;
using ExceptionHandlingDemo.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SystemException = ExceptionHandlingDemo.Exceptions.SystemException;

namespace ExceptionHandlingDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;
        private readonly RetryPolicyService _retryPolicyService;

        public TestController(ILogger<TestController> logger, RetryPolicyService retryPolicyService)
        {
            _logger = logger;
            _retryPolicyService = retryPolicyService;
        }
        [HttpGet("config-error")]
        public IActionResult GetConfigError()
        {
            throw new ConfigException("Configuration file is missing or invalid", 1001, HttpStatusCode.BadRequest);
        }

        [HttpGet("data-error")]
        public IActionResult GetDataError()
        {
            throw new DataException("Database connection failed", 2001, HttpStatusCode.ServiceUnavailable);
        }

        [HttpGet("logical-error")]
        public IActionResult GetLogicalError()
        {
            throw new LogicalException("Invalid business logic operation", 3001, HttpStatusCode.BadRequest);
        }

        [HttpGet("system-error")]
        public IActionResult GetSystemError()
        {
            throw new SystemException("System resource unavailable", 4001, HttpStatusCode.InternalServerError);
        }

        [HttpGet("standard-error")]
        public IActionResult GetStandardError()
        {
            // This will be caught by our middleware and converted to a system error
            throw new InvalidOperationException("This is a standard .NET exception");
        }

        [HttpGet("nested-error")]
        public IActionResult GetNestedError()
        {
            try
            {
                // Simulate a lower-level exception
                throw new InvalidOperationException("Lower level exception");
            }
            catch (Exception ex)
            {
                // Wrap it in our custom exception
                throw new LogicalException("A logical error occurred with an inner exception", 3002, ex, HttpStatusCode.BadRequest);
            }
        }

        [HttpGet("success")]
        public IActionResult GetSuccess()
        {
            return Ok(new { message = "Success! No exceptions here." });
        }

        [HttpGet("circuit-breaker")]
        public async Task<IActionResult> GetCircuitBreakerDemo()
        {
            try
            {
                // This will trigger the circuit breaker after multiple calls
                await _retryPolicyService.ExecuteAsync(async (ct) =>
                {
                    _logger.LogInformation("Executing operation that will fail...");
                    await Task.Delay(10, ct);
                    throw new SystemException("System resource unavailable for circuit breaker demo", 4002, HttpStatusCode.InternalServerError);
                });

                return Ok(new { message = "This should never be reached" });
            }
            catch
            {
                // The middleware will handle this exception
                throw;
            }
        }
    }
}
