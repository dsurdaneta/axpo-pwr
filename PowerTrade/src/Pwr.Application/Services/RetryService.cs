using Microsoft.Extensions.Logging;
using Pwr.Application.Interfaces;
using Pwr.Application.Models;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Pwr.Tests.UnitTests")]
namespace Pwr.Application.Services;

/// <summary>
/// Handles retry logic with configurable strategies.
/// </summary>
/// <param name="logger"></param>
public class RetryService(ILogger<RetryService> logger) : IRetryService
{
    private readonly ILogger<RetryService> _logger = logger;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">The type of reult</typeparam>
    /// <param name="operation">The operation to excecute</param>
    /// <param name="successCondition">Indicates whether the result was successful</param>
    /// <param name="context">Centralized configuration for retries and correlation</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<ExtractResult> ExecuteWithRetryAsync<T>(
        Func<Task<T>> operation,
        Func<T, bool> successCondition,
        ExtractContext context,
        CancellationToken cancellationToken = default)
    {
        var attempt = 0;
        Exception? lastException = null;

        while (attempt < context.MaxRetryAttempts)
        {
            attempt++;
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                _logger.LogInformation(
                    "Starting operation attempt {Attempt} for {RequestedUtc} [CorrelationId: {CorrelationId}]",
                    attempt, context.RequestedUtc, context.CorrelationId);

                var result = await operation();
                
                if (successCondition(result))
                {
                    _logger.LogInformation(
                        "Operation completed successfully on attempt {Attempt} for {RequestedUtc} [CorrelationId: {CorrelationId}]",
                        attempt, context.RequestedUtc, context.CorrelationId);
                    
                    return ExtractResult.Success(attempt);
                }

                _logger.LogWarning(
                    "Operation condition not met on attempt {Attempt} for {RequestedUtc} [CorrelationId: {CorrelationId}]",
                    attempt, context.RequestedUtc, context.CorrelationId);

                if (attempt < context.MaxRetryAttempts)
                {
                    await RetryWithDelay(context.RetryDelaySeconds, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation(
                    "Operation cancelled for {RequestedUtc} [CorrelationId: {CorrelationId}]",
                    context.RequestedUtc, context.CorrelationId);
                throw;
            }
            catch (Exception ex)
            {
                lastException = ex;
                _logger.LogError(ex,
                    "Error occurred during operation attempt {Attempt} for {RequestedUtc} [CorrelationId: {CorrelationId}]",
                    attempt, context.RequestedUtc, context.CorrelationId);

                if (attempt < context.MaxRetryAttempts)
                {
                    await RetryWithDelay(context.RetryDelaySeconds, cancellationToken);
                }
            }
        }

        var errorMessage = $"Operation failed after {context.MaxRetryAttempts} attempts";
        _logger.LogError(
            "All {MaxAttempts} operation attempts failed for {RequestedUtc} [CorrelationId: {CorrelationId}]",
            context.MaxRetryAttempts, context.RequestedUtc, context.CorrelationId);

        Console.WriteLine(errorMessage);

        return ExtractResult.Failure(attempt, errorMessage, lastException);
    }

    internal async Task RetryWithDelay(int retryDelaySeconds, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrying in {RetryDelay} seconds...", retryDelaySeconds);
        Console.WriteLine($"Retrying in {retryDelaySeconds} seconds...");
        await Task.Delay(TimeSpan.FromSeconds(retryDelaySeconds), cancellationToken);
    }
}
