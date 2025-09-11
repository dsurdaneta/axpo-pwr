using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pwr.Application.Interfaces;
using Pwr.Application.Models;
using Pwr.Application.Options;

namespace Pwr.Application.Services;

public class ScheduledExtractService : BackgroundService, IScheduledExtractService
{
    private readonly ILogger<ScheduledExtractService> _logger;
    private readonly IExtractService _extractService;
    private readonly IRetryService _retryService;
    private readonly ITimerService _timerService;
    private readonly IOptionsMonitor<ExtractTradesOptions> _extractOptions;
    private DateTime _lastExtractTime = DateTime.MinValue;

    public ScheduledExtractService(
        ILogger<ScheduledExtractService> logger,
        IExtractService extractService,
        IRetryService retryService,
        ITimerService timerService,
        IOptionsMonitor<ExtractTradesOptions> extractOptions)
    {
        _logger = logger;
        _extractService = extractService;
        _retryService = retryService;
        _timerService = timerService;
        _extractOptions = extractOptions;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ScheduledExtractService started");

        // Start the timer with the configured interval
        var intervalMs = GetIntervalMilliseconds();
        _timerService.Start(intervalMs, () => ExecuteExtractAsync(stoppingToken));

        _logger.LogInformation("Scheduled extract configured for every {IntervalMinutes} minutes", 
            _extractOptions.CurrentValue.ExtractIntervalMinutes);

        // Listen for configuration changes
        _extractOptions.OnChange(OnConfigurationChanged);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("ScheduledExtractService is stopping");
        }
    }

    private void OnConfigurationChanged(ExtractTradesOptions newOptions)
    {
        _logger.LogInformation("Configuration changed. New interval: {IntervalMinutes} minutes", newOptions.ExtractIntervalMinutes);
        
        var intervalMs = GetIntervalMilliseconds();
        _timerService.UpdateInterval(intervalMs);
    }

    private async Task ExecuteExtractAsync(CancellationToken cancellationToken)
    {
        try
        {
            var context = CreateExtractContext();
            var result = await _retryService.ExecuteWithRetryAsync(
                () => _extractService.PerformExtractAsync(context, cancellationToken),
                result => result.IsSuccess,
                context,
                cancellationToken);

            if (result.IsSuccess)
            {
                _lastExtractTime = DateTime.UtcNow;
                _logger.LogInformation(
                    "Extract completed successfully for {RequestedUtc} after {Attempts} attempts at {LastExtractTime} [CorrelationId: {CorrelationId}]",
                    context.RequestedUtc, result.AttemptsMade, _lastExtractTime, context.CorrelationId);
            }
            else
            {
                _logger.LogCritical(
                    "CRITICAL: Extract failed after {Attempts} attempts for {RequestedUtc}. Error: {ErrorMessage} [CorrelationId: {CorrelationId}]",
                    result.AttemptsMade, context.RequestedUtc, result.ErrorMessage, context.CorrelationId);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Extract operation was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred during extract execution");
        }
    }

    private ExtractContext CreateExtractContext()
    {
        var options = _extractOptions.CurrentValue;
        return new ExtractContext
        {
            RequestedUtc = DateTime.UtcNow.AddDays(1), // Request forecast for next day
            MaxRetryAttempts = options.MaxRetryAttempts,
            RetryDelaySeconds = options.RetryDelaySeconds
        };
    }

    private int GetIntervalMilliseconds() =>
        (int)TimeSpan.FromMinutes(_extractOptions.CurrentValue.ExtractIntervalMinutes).TotalMilliseconds;

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("ScheduledExtractService is stopping");
        
        _timerService.Stop();
        
        await base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        _timerService?.Dispose();
        base.Dispose();
    }
}
