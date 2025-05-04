# Exception Handling Demo

A comprehensive exception handling solution for ASP.NET Core applications with retry policies, circuit breaker patterns, metrics tracking, and enhanced error responses.

## Features

- **Centralized Exception Handling**: Global middleware to catch and process all exceptions
- **Custom Exception Types**: Categorized exceptions with error codes
- **Retry Policy**: Exponential backoff for system-level exceptions
- **Circuit Breaker Pattern**: Prevents cascading failures by breaking the circuit after multiple failures
- **Standardized Error Responses**: Consistent JSON error format across the application
- **Request Context Capture**: Detailed information about the request for debugging
- **Correlation IDs**: Track errors across systems and logs
- **Error Documentation**: Links to documentation and suggestions for resolving errors
- **Metrics and Monitoring**: Track error rates and types for analysis
- **Structured Logging**: Detailed context information in log entries

## Exception Categories

| Category | Error Code Range | Description |
|----------|------------------|-------------|
| Configuration | 1000-1999 | Configuration-related errors |
| Data | 2000-2999 | Database or data access errors |
| Logical | 3000-3999 | Business logic or validation errors |
| System | 4000-4999 | System resource or external service errors |

## Getting Started

### Prerequisites

- .NET 6.0 or later
- ASP.NET Core

### Installation

1. Clone the repository
2. Build the solution:
   ```
   dotnet build
   ```
3. Run the tests:
   ```
   dotnet test
   ```

## Usage

### Register the Middleware

In your `Program.cs` or `Startup.cs`:

```csharp
// Add services
builder.Services.AddSingleton<RetryPolicyService>();
builder.Services.AddSingleton<MetricsService>();

// Configure middleware
app.UseExceptionHandling();
```

### Throw Custom Exceptions

```csharp
// Configuration error
throw new ConfigException("Configuration file is missing", 1001);

// Data error
throw new DataException("Database connection failed", 2001);

// Logical error
throw new LogicalException("Invalid operation", 3001);

// System error (will trigger retry and circuit breaker)
throw new SystemException("External service unavailable", 4001);
```

### Error Response Format

```json
{
  "correlationId": "7b2f4a8e-1c3d-4e5f-9a8b-0c1d2e3f4a5b",
  "errorCode": 4001,
  "errorCategory": "System Issue",
  "httpStatusCode": 500,
  "message": "External service unavailable",
  "detailedMessage": "External service unavailable (Error Code: 4001)",
  "timestamp": "2023-07-20T15:30:45.123Z",
  "documentationUrl": "https://docs.example.com/errors/system-issue/4001",
  "suggestions": [
    "The system is experiencing issues. Please try again later.",
    "If the problem persists, contact support with the correlation ID."
  ],
  "requestContext": {
    "correlationId": "7b2f4a8e-1c3d-4e5f-9a8b-0c1d2e3f4a5b",
    "httpMethod": "GET",
    "path": "/api/resource",
    "queryString": "?param=value",
    "clientIp": "192.168.1.1",
    "userAgent": "Mozilla/5.0...",
    "userId": "user123",
    "requestTime": "2023-07-20T15:30:45.000Z"
  }
}
```

## Retry Policy

System exceptions (4000-4999) are automatically retried with exponential backoff:

- 1st retry: 100ms delay
- 2nd retry: 200ms delay
- 3rd retry: 400ms delay
- 4th retry: 800ms delay
- 5th retry: 1600ms delay

## Circuit Breaker

After 5 consecutive failures, the circuit opens for a specified duration, preventing further calls to the failing service.

## Metrics and Monitoring

The `MetricsService` provides methods to track and analyze error rates:

```csharp
// Get total error count
long totalErrors = _metricsService.GetTotalErrorCount();

// Get error rate (errors per minute) for a specific category
double dataErrorRate = _metricsService.GetCategoryErrorRate("Data Issue");

// Get error rate for a specific error code
double connectionErrorRate = _metricsService.GetErrorCodeRate(2001);

// Get a complete summary of error metrics
var summary = _metricsService.GetErrorMetricsSummary();
```

## Structured Logging

The middleware uses structured logging with correlation IDs:

```csharp
// Log entries include correlation ID and request context
using (_logger.BeginScope(new
{
    CorrelationId = requestContext.CorrelationId,
    ErrorCode = errorCode,
    ErrorCategory = errorCategory,
    Path = requestContext.Path,
    HttpMethod = requestContext.HttpMethod
}))
{
    _logger.LogError(
        exception,
        "Error handled: {ErrorCategory} {ErrorCode} - {Message}",
        errorCategory,
        errorCode,
        errorMessage);
}
```

## Running the Tests

```
dotnet test
```

## License

This project is licensed under the MIT License - see the LICENSE file for details.
