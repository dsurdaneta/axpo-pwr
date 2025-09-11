namespace Pwr.Application.Models;

/// <summary>
/// Result of a data extraction operation, including success status, attempts made, completion time, and error details if applicable.
/// </summary>
public class ExtractResult
{
    public bool IsSuccess { get; init; }
    public int AttemptsMade { get; init; }
    public DateTime CompletedAt { get; init; }
    public string? ErrorMessage { get; init; }
    public Exception? Exception { get; init; }

    public static ExtractResult Success(int attemptsMade) => new()
    {
        IsSuccess = true,
        AttemptsMade = attemptsMade,
        CompletedAt = DateTime.UtcNow
    };

    public static ExtractResult Failure(int attemptsMade, string errorMessage, Exception? exception = null) => new()
    {
        IsSuccess = false,
        AttemptsMade = attemptsMade,
        CompletedAt = DateTime.UtcNow,
        ErrorMessage = errorMessage,
        Exception = exception
    };
}
