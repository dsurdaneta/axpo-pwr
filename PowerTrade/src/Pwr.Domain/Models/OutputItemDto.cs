namespace Pwr.Domain.Models;

public class OutputItemDto
{
    public string DateTime { get; set; } = string.Empty;
    public double Volume { get; set; }

    public static OutputItemDto FromInputItemDto(InputItemDto input) 
        => new()
        {
            DateTime = input.DateTime.ToUniversalIso8601(),
            Volume = input.Volume
        };
}
