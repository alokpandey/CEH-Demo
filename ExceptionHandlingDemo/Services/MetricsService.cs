/*
 * File: MetricsService.cs
 * Project: Exception Handling Demo
 * Created: May 2024
 *
 * Description:
 * Service for tracking application metrics including error rates and types.
 * Provides methods to record errors, calculate error rates, and generate
 * error metrics summaries. Includes automatic cleanup of old error data.
 *
 * Copyright (c) 2024. All rights reserved.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace ExceptionHandlingDemo.Services
{
    /// <summary>
    /// Service for tracking application metrics including errors
    /// </summary>
    public class MetricsService
    {
        private readonly ILogger<MetricsService> _logger;
        private readonly ConcurrentDictionary<string, long> _errorCounters = new();
        private readonly ConcurrentDictionary<string, ConcurrentQueue<DateTime>> _errorTimestamps = new();
        private readonly TimeSpan _errorRateWindow = TimeSpan.FromMinutes(5);
        private readonly Timer _cleanupTimer;

        public MetricsService(ILogger<MetricsService> logger)
        {
            _logger = logger;

            // Create a timer to clean up old error timestamps
            _cleanupTimer = new Timer(CleanupOldErrors, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
        }

        /// <summary>
        /// Records an error occurrence
        /// </summary>
        /// <param name="errorCategory">The category of the error</param>
        /// <param name="errorCode">The specific error code</param>
        /// <param name="correlationId">The correlation ID for tracking</param>
        public void RecordError(string errorCategory, int errorCode, string correlationId)
        {
            // Increment total error count
            _errorCounters.AddOrUpdate("total", 1, (_, count) => count + 1);

            // Increment category error count
            string categoryKey = $"category:{errorCategory}";
            _errorCounters.AddOrUpdate(categoryKey, 1, (_, count) => count + 1);

            // Increment specific error code count
            string codeKey = $"code:{errorCode}";
            _errorCounters.AddOrUpdate(codeKey, 1, (_, count) => count + 1);

            // Add timestamp for error rate calculations
            var now = DateTime.UtcNow;

            // Add to total error rate queue
            if (!_errorTimestamps.TryGetValue("total", out var totalQueue))
            {
                totalQueue = new ConcurrentQueue<DateTime>();
                _errorTimestamps.TryAdd("total", totalQueue);
            }
            totalQueue.Enqueue(now);

            // Add to category error rate queue
            if (!_errorTimestamps.TryGetValue(categoryKey, out var categoryQueue))
            {
                categoryQueue = new ConcurrentQueue<DateTime>();
                _errorTimestamps.TryAdd(categoryKey, categoryQueue);
            }
            categoryQueue.Enqueue(now);

            // Add to code error rate queue
            if (!_errorTimestamps.TryGetValue(codeKey, out var codeQueue))
            {
                codeQueue = new ConcurrentQueue<DateTime>();
                _errorTimestamps.TryAdd(codeKey, codeQueue);
            }
            codeQueue.Enqueue(now);

            // Log the error for metrics purposes
            _logger.LogInformation(
                "Error metrics: Category={Category}, Code={Code}, CorrelationId={CorrelationId}",
                errorCategory, errorCode, correlationId);
        }

        /// <summary>
        /// Gets the total number of errors recorded
        /// </summary>
        public long GetTotalErrorCount()
        {
            return _errorCounters.TryGetValue("total", out var count) ? count : 0;
        }

        /// <summary>
        /// Gets the number of errors for a specific category
        /// </summary>
        public long GetCategoryErrorCount(string category)
        {
            string key = $"category:{category}";
            return _errorCounters.TryGetValue(key, out var count) ? count : 0;
        }

        /// <summary>
        /// Gets the number of errors for a specific error code
        /// </summary>
        public long GetErrorCodeCount(int errorCode)
        {
            string key = $"code:{errorCode}";
            return _errorCounters.TryGetValue(key, out var count) ? count : 0;
        }

        /// <summary>
        /// Gets the error rate (errors per minute) for all errors
        /// </summary>
        public double GetTotalErrorRate()
        {
            return CalculateErrorRate("total");
        }

        /// <summary>
        /// Gets the error rate (errors per minute) for a specific category
        /// </summary>
        public double GetCategoryErrorRate(string category)
        {
            string key = $"category:{category}";
            return CalculateErrorRate(key);
        }

        /// <summary>
        /// Gets the error rate (errors per minute) for a specific error code
        /// </summary>
        public double GetErrorCodeRate(int errorCode)
        {
            string key = $"code:{errorCode}";
            return CalculateErrorRate(key);
        }

        /// <summary>
        /// Gets a summary of all error metrics
        /// </summary>
        public Dictionary<string, object> GetErrorMetricsSummary()
        {
            var summary = new Dictionary<string, object>
            {
                ["totalErrors"] = GetTotalErrorCount(),
                ["errorRate"] = GetTotalErrorRate(),
                ["categoryBreakdown"] = GetCategoryBreakdown(),
                ["topErrorCodes"] = GetTopErrorCodes(5)
            };

            return summary;
        }

        /// <summary>
        /// Gets a breakdown of errors by category
        /// </summary>
        private Dictionary<string, long> GetCategoryBreakdown()
        {
            var breakdown = new Dictionary<string, long>();

            foreach (var key in _errorCounters.Keys.Where(k => k.StartsWith("category:")))
            {
                string category = key.Substring("category:".Length);
                breakdown[category] = _errorCounters[key];
            }

            return breakdown;
        }

        /// <summary>
        /// Gets the top N error codes by count
        /// </summary>
        private List<KeyValuePair<int, long>> GetTopErrorCodes(int count)
        {
            var errorCodes = new List<KeyValuePair<int, long>>();

            foreach (var key in _errorCounters.Keys.Where(k => k.StartsWith("code:")))
            {
                if (int.TryParse(key.Substring("code:".Length), out var code))
                {
                    errorCodes.Add(new KeyValuePair<int, long>(code, _errorCounters[key]));
                }
            }

            return errorCodes.OrderByDescending(kv => kv.Value).Take(count).ToList();
        }

        /// <summary>
        /// Calculates the error rate for a specific key
        /// </summary>
        private double CalculateErrorRate(string key)
        {
            if (!_errorTimestamps.TryGetValue(key, out var queue))
            {
                return 0;
            }

            var cutoff = DateTime.UtcNow.Subtract(_errorRateWindow);
            var recentErrors = queue.Where(ts => ts >= cutoff).Count();

            // Calculate errors per minute
            return recentErrors / _errorRateWindow.TotalMinutes;
        }

        /// <summary>
        /// Cleans up old error timestamps
        /// </summary>
        private void CleanupOldErrors(object? state)
        {
            try
            {
                var cutoff = DateTime.UtcNow.Subtract(_errorRateWindow);

                foreach (var key in _errorTimestamps.Keys)
                {
                    if (_errorTimestamps.TryGetValue(key, out var queue))
                    {
                        // Create a new queue with only recent errors
                        var newQueue = new ConcurrentQueue<DateTime>(queue.Where(ts => ts >= cutoff));

                        // Replace the old queue with the new one
                        _errorTimestamps[key] = newQueue;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up old error timestamps");
            }
        }
    }
}
