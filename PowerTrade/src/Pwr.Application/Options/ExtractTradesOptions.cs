namespace Pwr.Application.Options;

public class ExtractTradesOptions
{
    public const string SectionName = "ExtractTradesOptions";

    public string FolderPath { get; set; } = string.Empty;
    public int ExtractIntervalMinutes { get; set; } = 60;
    public int MaxRetryAttempts { get; set; } = 3;
    public int RetryDelaySeconds { get; set; } = 30;
}
