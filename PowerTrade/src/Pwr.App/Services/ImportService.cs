using Axpo;
using Microsoft.Extensions.Logging;
using Pwr.App.Models;

namespace Pwr.App.Services;

public class ImportService
{
    private readonly ILogger<ImportService> _logger;
    private readonly IPowerService _powerService;

    public ImportService(ILogger<ImportService> logger, IPowerService powerService)
    {
        _logger = logger;
        _powerService = powerService;
    }

    public async Task<IEnumerable<PowerTrade>> GetForcastTradesFromExternalService(DateTime requestedUtc)
    {
        //TODO 
        //Read from config
        //implement interface
        //DST validation
        //Dependency injection
        _logger.LogInformation("Starting to get trades from external service for date {RequestedDate}", requestedUtc);

        IEnumerable<PowerTrade> trades = new List<PowerTrade>();

        try
        {
            trades = await _powerService.GetTradesAsync(requestedUtc);
            _logger.LogInformation("Successfully retrieved {TradeCount} trades from external service", trades.Count());
        }
        catch (Axpo.PowerServiceException ex)
        {
            _logger.LogError(ex, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving trades from external service");
        }

        return trades;
    }

    public List<OutputItemDto> MapTrades(DateTime requestedUtc, IEnumerable<PowerTrade> trades)
    {
        var rows = new List<OutputItemDto>();
        if (!trades.Any())
        {
            _logger.LogWarning("No trades retrieved for date {RequestedDate}", requestedUtc);
            return rows;
        }

        var firstTrade = trades.FirstOrDefault();
        var periodsCount = firstTrade.Periods.Count();
        _logger.LogInformation("Periods count: {PeriodsCount} for date {RequestedDate}", periodsCount, requestedUtc);

        for (int i = 0; i < firstTrade.Periods.Length; i++)
        {
            var calculatedVolume = trades.Sum(t => t.Periods[i].Volume);
            var periodCounter = i + 1;
            var auxDate = new DateTime(requestedUtc.Year, requestedUtc.Month, requestedUtc.Day, requestedUtc.Hour, 0, 0);
            var periodAsDateTime = auxDate.AddHours(periodCounter);
            rows.Add(new OutputItemDto { DateTime = periodAsDateTime, Volume = calculatedVolume });
        }

        return rows;
    }
}
