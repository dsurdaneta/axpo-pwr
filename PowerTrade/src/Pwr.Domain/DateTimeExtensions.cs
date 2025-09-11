namespace Pwr.Domain;

public static class DateTimeExtensions
{
    public static string ToUniversalIso8601string(this DateTime dateTime) =>
        dateTime.ToUniversalTime().ToString("u").Replace(" ", "T");
}
