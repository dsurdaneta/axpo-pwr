using Axpo;
using CsvHelper;
using CsvHelper.Configuration;
using Pwr.App.Models;
using System.Globalization;
using System.Text;

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

        var rows = new List<OutputItemDto>();

        for (int i = 0; i < firstTrade.Periods.Length; i++)
        {
            var calculatedVolume = trades.Sum(t => t.Periods[i].Volume);
            var periodCounter = i + 1;
            var auxDate = new DateTime(requestedUtc.Year, requestedUtc.Month, requestedUtc.Day, requestedUtc.Hour, 0, 0);
            var periodAsDateTime = auxDate.AddHours(periodCounter);
            Console.WriteLine($"{periodCounter} - DateTime {periodAsDateTime} - Volume {calculatedVolume}");
            rows.Add(new OutputItemDto { DateTime = periodAsDateTime, Volume = calculatedVolume });
        }

        try
        {
            Console.WriteLine("Writing to CSV...");
            var config = new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";", Encoding = Encoding.UTF8 };
            var datePart = requestedUtc.ToString(Constants.FileFormat);
            var fileName = $"{Constants.FilePrefix}_{datePart}.csv";
            using (var writer = new StreamWriter($"C:\\Users\\DanielUrdanetaOropez\\source\\{fileName}"))
            using (var csv = new CsvWriter(writer, config))
            {
                csv.WriteRecords(rows);
            }
            Console.WriteLine($"File {fileName} created successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
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
