namespace Pwr.Application.Models;

/// <summary>
/// Context information for data extraction operations. Centralized configuration for retries and correlation.
/// </summary>
public class ExtractContext
{
    public DateTime RequestedUtc { get; init; }
    public int MaxRetryAttempts { get; init; }
    public int RetryDelaySeconds { get; init; }
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString("N")[..8];
}
