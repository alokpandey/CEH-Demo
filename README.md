# Exception Handling Demo

A comprehensive exception handling solution for ASP.NET Core applications with retry policies and circuit breaker patterns.

## Features

- **Centralized Exception Handling**: Global middleware to catch and process all exceptions
- **Custom Exception Types**: Categorized exceptions with error codes
- **Retry Policy**: Exponential backoff for system-level exceptions
- **Circuit Breaker Pattern**: Prevents cascading failures by breaking the circuit after multiple failures
- **Standardized Error Responses**: Consistent JSON error format across the application

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
  "errorCode": 4001,
  "errorCategory": "System Issue",
  "message": "External service unavailable",
  "timestamp": "2023-07-20T15:30:45.123Z",
  "path": "/api/resource"
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

## Running the Tests

```
dotnet test
```

## License

This project is licensed under the MIT License - see the LICENSE file for details.
