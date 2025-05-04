# Exception Handling Middleware Test Cases

This document provides detailed information about the test cases for the Exception Handling Middleware. It explains what each test case is testing, how it's being tested, and what assertions are being made.

## Test Environment Setup

The tests use ASP.NET Core's `TestServer` to create an in-memory server that processes HTTP requests without the need for a real HTTP server. This allows us to test the middleware in isolation.

### Key Components:

1. **TestServer**: An in-memory server that simulates HTTP requests
2. **HttpClient**: Used to make requests to the TestServer
3. **TestStartup**: A custom startup class that configures the test environment
4. **TestController**: Contains endpoints that throw different types of exceptions

## Test Cases

### 1. Configuration Error Test

**Test Method**: `ConfigError_ReturnsCorrectErrorResponse()`

**What it tests**:
- Verifies that the middleware correctly handles `ConfigException` (error code 1000-1999)
- Ensures the error response contains all required fields with correct values
- Checks that the HTTP status code is set to BadRequest (400)

**How it works**:
1. Makes a GET request to `/api/test/config-error` which throws a `ConfigException`
2. Deserializes the response into an `ErrorResponse` object
3. Verifies all fields including correlation ID, error code, category, message, etc.

**Key assertions**:
- HTTP status code is 400 (BadRequest)
- Error code is 1001
- Error category is "Configuration Error"
- Error message is "Configuration file is missing or invalid"
- All required fields (correlation ID, timestamp, path, etc.) are present

### 2. Data Error Test

**Test Method**: `DataError_ReturnsCorrectErrorResponse()`

**What it tests**:
- Verifies that the middleware correctly handles `DataException` (error code 2000-2999)
- Ensures the error response contains the correct error code, category, and message
- Checks that the HTTP status code is set to ServiceUnavailable (503)

**How it works**:
1. Makes a GET request to `/api/test/data-error` which throws a `DataException`
2. Deserializes the response into an `ErrorResponse` object
3. Verifies key fields like status code, error code, category, and message

**Key assertions**:
- HTTP status code is 503 (ServiceUnavailable)
- Error code is 2001
- Error category is "Data Issue"
- Error message is "Database connection failed"

### 3. Logical Error Test

**Test Method**: `LogicalError_ReturnsCorrectErrorResponse()`

**What it tests**:
- Verifies that the middleware correctly handles `LogicalException` (error code 3000-3999)
- Ensures the error response contains the correct error code, category, and message
- Checks that the HTTP status code is set to BadRequest (400)

**How it works**:
1. Makes a GET request to `/api/test/logical-error` which throws a `LogicalException`
2. Deserializes the response into an `ErrorResponse` object
3. Verifies key fields like status code, error code, category, and message

**Key assertions**:
- HTTP status code is 400 (BadRequest)
- Error code is 3001
- Error category is "Logical Application Bug"
- Error message is "Invalid business logic operation"

### 4. System Error Test

**Test Method**: `SystemError_ReturnsCorrectErrorResponse()`

**What it tests**:
- Verifies that the middleware correctly handles `SystemException` (error code 4000-4999)
- Ensures the error response contains the correct error code, category, and message
- Checks that the HTTP status code is set to InternalServerError (500)

**How it works**:
1. Makes a GET request to `/api/test/system-error` which throws a `SystemException`
2. Deserializes the response into an `ErrorResponse` object
3. Verifies key fields like status code, error code, category, and message

**Key assertions**:
- HTTP status code is 500 (InternalServerError)
- Error code is 4001
- Error category is "System Issue"
- Error message is "System resource unavailable"

### 5. Standard .NET Exception Test

**Test Method**: `StandardError_ReturnsSystemErrorResponse()`

**What it tests**:
- Verifies that the middleware correctly handles standard .NET exceptions (not our custom exceptions)
- Ensures these exceptions are converted to a system error with code 4000
- Checks that the HTTP status code is set to InternalServerError (500)

**How it works**:
1. Makes a GET request to `/api/test/standard-error` which throws a standard `InvalidOperationException`
2. Deserializes the response into an `ErrorResponse` object
3. Verifies that it's treated as a system error

**Key assertions**:
- HTTP status code is 500 (InternalServerError)
- Error code is 4000 (default system error code)
- Error category is "System Issue"

### 6. Success Response Test

**Test Method**: `SuccessEndpoint_ReturnsOk()`

**What it tests**:
- Verifies that the middleware doesn't interfere with successful responses
- Ensures that endpoints that don't throw exceptions work correctly

**How it works**:
1. Makes a GET request to `/api/test/success` which returns a 200 OK response
2. Verifies that the status code is OK

**Key assertions**:
- HTTP status code is 200 (OK)

### 7. Circuit Breaker Test

**Test Method**: `CircuitBreaker_AfterMultipleCalls_ReturnsCircuitBreakerException()`

**What it tests**:
- Verifies that the circuit breaker pattern works correctly after multiple failures
- Ensures that after the circuit is open, subsequent calls return a circuit breaker exception
- Checks that the error response contains the correct category and message

**How it works**:
1. Makes multiple calls (6) to `/api/test/circuit-breaker` to trigger the circuit breaker
2. Makes one more call after the circuit should be open
3. Verifies that this call returns a circuit breaker exception

**Key assertions**:
- HTTP status code is either 503 (ServiceUnavailable) or 500 (InternalServerError)
- Error category is "System Issue"
- Error message is not null or empty

## How to Run the Tests

To run all tests:

```bash
dotnet test
```

To run a specific test:

```bash
dotnet test --filter "FullyQualifiedName=ExceptionHandlingDemo.Tests.ExceptionHandlingMiddlewareTests.CircuitBreaker_AfterMultipleCalls_ReturnsCircuitBreakerException"
```

## Test Coverage

These tests cover:

1. All custom exception types (Configuration, Data, Logical, System)
2. Standard .NET exceptions
3. Successful responses
4. Circuit breaker functionality

The tests verify:
- Correct HTTP status codes
- Correct error codes and categories
- Presence of required fields in the error response
- Proper functioning of the retry and circuit breaker patterns

## Adding New Tests

When adding new tests, follow this pattern:

1. Create a new test method with a descriptive name
2. Make a request to an endpoint that triggers the behavior you want to test
3. Deserialize the response into an `ErrorResponse` object
4. Assert that the response contains the expected values
5. Add detailed comments explaining what the test is verifying
