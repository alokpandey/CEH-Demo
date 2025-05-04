/*
 * File: RetryPolicyService.cs
 * Project: Exception Handling Demo
 * Created: May 2024
 *
 * Description:
 * Service for implementing retry policies with exponential backoff and circuit breaker patterns.
 * Provides methods to execute operations with automatic retries for transient failures
 * and circuit breaking to prevent cascading failures after multiple consecutive errors.
 *
 * Copyright (c) 2024. All rights reserved.
 */

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Wrap;

namespace ExceptionHandlingDemo.Services
{
    /// <summary>
    /// Service for handling retries with exponential backoff and circuit breaker pattern
    /// </summary>
    public class RetryPolicyService
    {
        private readonly ILogger<RetryPolicyService> _logger;
        private readonly AsyncRetryPolicy _retryPolicy;
        private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;
        private readonly AsyncPolicyWrap _policyWrap;

        // Circuit breaker state
        public bool IsCircuitBroken => _circuitBreakerPolicy.CircuitState == CircuitState.Open;
        public CircuitState CircuitState => _circuitBreakerPolicy.CircuitState;

        public RetryPolicyService(ILogger<RetryPolicyService> logger)
        {
            _logger = logger;

            // Create retry policy with exponential backoff
            _retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    retryCount: 5, // 5 retries
                    sleepDurationProvider: retryAttempt => TimeSpan.FromMilliseconds(50 * Math.Pow(2, retryAttempt)), // Much shorter for tests: 50ms, 100ms, 200ms, 400ms, 800ms
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(
                            exception,
                            "Retry {RetryCount} after {RetryDelay}s delay due to: {ExceptionMessage}",
                            retryCount,
                            timeSpan.TotalSeconds,
                            exception.Message);
                    });

            // Create circuit breaker policy
            _circuitBreakerPolicy = Policy
                .Handle<Exception>()
                .CircuitBreakerAsync(
                    exceptionsAllowedBeforeBreaking: 5, // Break after 5 consecutive failures
                    durationOfBreak: TimeSpan.FromMilliseconds(500), // Circuit stays open for 500ms (for testing)
                    onBreak: (exception, timeSpan) =>
                    {
                        _logger.LogError(
                            exception,
                            "Circuit breaker opened for {DurationOfBreak}s due to: {ExceptionMessage}",
                            timeSpan.TotalSeconds,
                            exception.Message);
                    },
                    onReset: () =>
                    {
                        _logger.LogInformation("Circuit breaker reset");
                    },
                    onHalfOpen: () =>
                    {
                        _logger.LogInformation("Circuit breaker half-open, next call is a trial");
                    });

            // Combine policies: first retry, then circuit breaker
            _policyWrap = Policy.WrapAsync(_circuitBreakerPolicy, _retryPolicy);
        }

        /// <summary>
        /// Executes the specified action with retry and circuit breaker policies
        /// </summary>
        /// <param name="action">The action to execute</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default)
        {
            await _policyWrap.ExecuteAsync(async (ct) => await action(ct), cancellationToken);
        }

        /// <summary>
        /// Executes the specified function with retry and circuit breaker policies
        /// </summary>
        /// <typeparam name="T">The return type of the function</typeparam>
        /// <param name="func">The function to execute</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The result of the function</returns>
        public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> func, CancellationToken cancellationToken = default)
        {
            return await _policyWrap.ExecuteAsync(async (ct) => await func(ct), cancellationToken);
        }
    }
}
