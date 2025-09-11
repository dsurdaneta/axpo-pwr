using Microsoft.Extensions.Logging;
using Pwr.Application.Interfaces;
using Pwr.Application.Models;

namespace Pwr.Application.Services;

public class RetryService(ILogger<RetryService> logger) : IRetryService
{
    private readonly ILogger<RetryService> _logger = logger;

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

        return ExtractResult.Failure(attempt, errorMessage, lastException);
    }

    private async Task RetryWithDelay(int retryDelaySeconds, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrying in {RetryDelay} seconds...", retryDelaySeconds);
        await Task.Delay(TimeSpan.FromSeconds(retryDelaySeconds), cancellationToken);
    }
}
