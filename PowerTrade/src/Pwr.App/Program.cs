using Axpo;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pwr.App.Models;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Pwr.App;

internal class Program
{
    static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        //var requiredService = host.Services.GetRequiredService<ISomeServiceInterface>();

        string appEnvPath = Environment.CurrentDirectory;
        string appDomainPath = AppDomain.CurrentDomain.BaseDirectory;
        string execAssemblypPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string entrycAssemblypPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        Console.WriteLine($"Environment Path: {appEnvPath}");
        Console.WriteLine($"App Domain Path: {appDomainPath}");
        Console.WriteLine($"Exec Assembly Path: {execAssemblypPath}");
        Console.WriteLine($"Entry Assembly Path: {entrycAssemblypPath}");
        Console.WriteLine();

        var requestedUtc = DateTime.UtcNow.AddDays(1); // should request the forcast for the next day

        IEnumerable<PowerTrade> trades = await GetTradesFromExternalService(requestedUtc);
        var rows = MapTrades(requestedUtc, trades);

        GenerateReport(requestedUtc, rows);

        Console.WriteLine("Press any key");
        Console.ReadLine();
    }

    private static List<OutputItemDto> MapTrades(DateTime requestedUtc, IEnumerable<PowerTrade> trades)
    {
        var rows = new List<OutputItemDto>();
        if (!trades.Any())
        {
            Console.WriteLine("No trades retrieved. Exiting application.");
            return rows;
        }

        var firstTrade = trades.FirstOrDefault();
        var periodsCount = firstTrade.Periods.Count();
        Console.WriteLine($"Periods count: {periodsCount}");
        
        for (int i = 0; i < firstTrade.Periods.Length; i++)
        {
            var calculatedVolume = trades.Sum(t => t.Periods[i].Volume);
            var periodCounter = i + 1;
            var auxDate = new DateTime(requestedUtc.Year, requestedUtc.Month, requestedUtc.Day, requestedUtc.Hour, 0, 0);
            var periodAsDateTime = auxDate.AddHours(periodCounter);
            Console.WriteLine($"{periodCounter} - DateTime {periodAsDateTime} - Volume {calculatedVolume}");
            rows.Add(new OutputItemDto { DateTime = periodAsDateTime, Volume = calculatedVolume });
        }

        return rows;
    }

    private static void GenerateReport(DateTime requestedUtc, List<OutputItemDto> rows)
    {
        try
        {
            //TODO create output folder if does not exists
            //Delimiter as constant
            //Read from config
            //add logs
            Console.WriteLine("Writing to CSV...");
            var config = new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";", Encoding = Encoding.UTF8 };
            var datePart = requestedUtc.ToString(Constants.FileFormat);
            var fileName = $"{Constants.FilePrefix}_{datePart}.csv";
            var basePath = $"C:\\Users\\DanielUrdanetaOropez\\source";
            using (var writer = new StreamWriter($"{basePath}\\{fileName}"))
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
    }

    private static async Task<IEnumerable<PowerTrade>> GetTradesFromExternalService(DateTime requestedUtc)
    {
        //TODO 
        //Read from config
        //add logs
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

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory())
                      .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                      .AddEnvironmentVariables();
            }).ConfigureServices((context, services) =>
            {
                services.AddLogging(builder =>
                {
                    //On prod we can set a different log configuration
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Debug);
                });

                //TODO register solution services
            });
}
