using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pwr.Application.Interfaces;
using Pwr.Application.Options;
using Pwr.Domain.Models;

namespace Pwr.Application.Services;

public class ScheduledExtractService : BackgroundService, IScheduledExtractService
{
    private readonly ILogger<ScheduledExtractService> _logger;
    private readonly IForcastCallingService _forcastCallingService;
    private readonly IExportService _exportService;
    private readonly IOptionsMonitor<ExtractTradesOptions> _extractOptions;
    private readonly Timer _timer;
    private readonly object _lockObject = new();
    private bool _isRunning = false;
    private DateTime _lastExtractTime = DateTime.MinValue;

    public ScheduledExtractService(
        ILogger<ScheduledExtractService> logger,
        IForcastCallingService forcastCallingService,
        IExportService exportService,
        IOptionsMonitor<ExtractTradesOptions> extractOptions)
    {
        _logger = logger;
        _forcastCallingService = forcastCallingService;
        _exportService = exportService;
        _extractOptions = extractOptions;
        _timer = new Timer(ExecuteExtract, null, Timeout.Infinite, Timeout.Infinite);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ScheduledExtractService started");

        // Start the timer with the configured interval
        var intervalMinutes = _extractOptions.CurrentValue.ExtractIntervalMinutes;
        var intervalMs = (int)TimeSpan.FromMinutes(intervalMinutes).TotalMilliseconds;
        
        _timer.Change(intervalMs, intervalMs);

        _logger.LogInformation("Scheduled extract configured for every {IntervalMinutes} minutes", intervalMinutes);

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
        
        var intervalMs = (int)TimeSpan.FromMinutes(newOptions.ExtractIntervalMinutes).TotalMilliseconds;
        
        _timer.Change(intervalMs, intervalMs);
    }

    private async void ExecuteExtract(object? state)
    {
        lock (_lockObject)
        {
            if (_isRunning)
            {
                _logger.LogWarning("Previous extract is still running, skipping this scheduled execution");
                return;
            }
            _isRunning = true;
        }

        try
        {
            await PerformExtractWithRetry();
        }
        finally
        {
            lock (_lockObject)
            {
                _isRunning = false;
            }
        }
    }

    private async Task PerformExtractWithRetry()
    {
        var requestedUtc = DateTime.UtcNow;
        var attempt = 0;
        var success = false;
        var maxRetryAttempts = _extractOptions.CurrentValue.MaxRetryAttempts;
        var retryDelaySeconds = _extractOptions.CurrentValue.RetryDelaySeconds;

        while (attempt < maxRetryAttempts && !success)
        {
            attempt++;
            
            try
            {
                _logger.LogInformation("Starting extract attempt {Attempt} for {RequestedUtc}", attempt, requestedUtc);
                
                // Get forecast data
                var forecastData = await _forcastCallingService.GetForcastAsync(requestedUtc);
                var forecastList = forecastData.ToList();

                if (forecastList.Count == 0)
                {
                    _logger.LogWarning("No forecast data retrieved for {RequestedUtc}, attempt {Attempt}", requestedUtc, attempt);
                    
                    if (attempt < maxRetryAttempts)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(retryDelaySeconds));
                        continue;
                    }
                    else
                    {
                        _logger.LogError("Failed to retrieve forecast data after {MaxAttempts} attempts", maxRetryAttempts);
                        return;
                    }
                }

                // Generate report
                var reportGenerated = _exportService.GenerateReport(requestedUtc, forecastList);
                
                if (reportGenerated)
                {
                    _logger.LogInformation("Extract completed successfully for {RequestedUtc} on attempt {Attempt}", requestedUtc, attempt);
                    _lastExtractTime = DateTime.UtcNow;
                    success = true;
                }
                else
                {
                    _logger.LogError("Failed to generate report for {RequestedUtc}, attempt {Attempt}", requestedUtc, attempt);
                    
                    if (attempt < maxRetryAttempts)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(retryDelaySeconds));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during extract attempt {Attempt} for {RequestedUtc}", attempt, requestedUtc);
                
                if (attempt < maxRetryAttempts)
                {
                    _logger.LogInformation("Retrying in {RetryDelay} seconds...", retryDelaySeconds);
                    await Task.Delay(TimeSpan.FromSeconds(retryDelaySeconds));
                }
                else
                {
                    _logger.LogError("All {MaxAttempts} extract attempts failed for {RequestedUtc}", maxRetryAttempts, requestedUtc);
                }
            }
        }

        if (!success)
        {
            _logger.LogCritical("CRITICAL: Extract failed after all retry attempts for {RequestedUtc}. Manual intervention may be required.", requestedUtc);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("ScheduledExtractService is stopping");
        
        _timer.Change(Timeout.Infinite, Timeout.Infinite);
        _timer.Dispose();
        
        await base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        _timer?.Dispose();
        base.Dispose();
    }
}
