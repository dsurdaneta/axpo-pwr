using Axpo;
using Pwr.App.Models;

namespace Pwr.App;

internal class Program
{
    static async Task Main(string[] args)
    {
        var requestedUtc = DateTime.UtcNow.AddDays(1); // should request the forcast for the next day

        IEnumerable<PowerTrade> trades = await GetTradesFromExternalService(requestedUtc);

        var firstTrade = trades.FirstOrDefault();
        var periodsCount = firstTrade.Periods.Count();
        Console.WriteLine($"Periods count: {periodsCount}");

        var rows = new List<OutputDto>();

        for (int i = 0; i < firstTrade.Periods.Length; i++)
        {
            var calculatedVolume = trades.Sum(t => t.Periods[i].Volume);
            var periodCounter = i + 1;
            var auxDate = new DateTime(requestedUtc.Year, requestedUtc.Month, requestedUtc.Day, requestedUtc.Hour, 0, 0);
            var periodAsDateTime = auxDate.AddHours(periodCounter);
            Console.WriteLine($"{periodCounter} - DateTime {periodAsDateTime} - Volume {calculatedVolume}");
            rows.Add(new OutputDto { DateTime = periodAsDateTime, Volume = calculatedVolume });
        }

        Console.WriteLine("Press any key");
        Console.ReadLine();
    }

    private static async Task<IEnumerable<PowerTrade>> GetTradesFromExternalService(DateTime requestedUtc)
    {
        var trades = Enumerable.Empty<PowerTrade>();
        try
        {
            IPowerService service = new PowerService();
            trades = await service.GetTradesAsync(requestedUtc);
            Console.WriteLine($"Date Requested in UTC: {requestedUtc}");
            Console.WriteLine($"Trades count: {trades.Count()}");
            return trades;
        }
        catch (Axpo.PowerServiceException ex)
        {
            Console.WriteLine(ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        return trades;
    }
}
