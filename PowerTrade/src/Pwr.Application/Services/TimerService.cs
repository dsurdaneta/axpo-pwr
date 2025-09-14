using Microsoft.Extensions.Logging;
using Pwr.Application.Interfaces;

namespace Pwr.Application.Services;

/// <summary>
/// Manages timer operations and thread safety
/// </summary>
/// <param name="logger"></param>
public class TimerService(ILogger<TimerService> logger) : ITimerService
{
    private readonly ILogger<TimerService> _logger = logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private Timer? _timer;
    private Func<Task>? _callback;
    private bool _disposed;

    public void Start(int intervalMs, Func<Task> callback)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(TimerService));

        _callback = callback;
        _timer?.Dispose();
        _timer = new Timer(ExecuteCallback, null, intervalMs, intervalMs);
        
        _logger.LogInformation("Timer started with interval {IntervalMs}ms", intervalMs);
    }

    public void Stop()
    {
        if (_disposed) return;

        _timer?.Change(Timeout.Infinite, Timeout.Infinite);
        _logger.LogInformation("Timer stopped");
    }

    public void UpdateInterval(int intervalMs)
    {
        if (_disposed) return;

        _timer?.Change(intervalMs, intervalMs);
        _logger.LogInformation("Timer interval updated to {IntervalMs}ms", intervalMs);
    }

    internal async void ExecuteCallback(object? state)
    {
        if (_disposed || _callback == null) return;

        if (!await _semaphore.WaitAsync(100)) // Wait max 100ms
        {
            _logger.LogWarning("Previous timer callback is still running, skipping this execution");
            return;
        }

        try
        {
            await _callback();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred in timer callback");
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;
        _timer?.Dispose();
        _semaphore?.Dispose();
        _logger.LogInformation("TimerService disposed");
    }
}
