namespace Pwr.App.Options;

public class CsvOptions
{
    public const string SectionName = "CsvOptions";

    public string FolderPath { get; set; } = string.Empty;
    public int ExtractIntervalMinutes { get; set; } = 15;
}
