using Axpo;
using Microsoft.Extensions.Logging;
using Pwr.Application.Interfaces;
using Pwr.Domain.Models;

namespace Pwr.Application.Services;

public class ForcastCallingService : IForcastCallingService
{
    private readonly ILogger<ForcastCallingService> _logger;
    private readonly IPowerService _powerService;

    public ForcastCallingService(ILogger<ForcastCallingService> logger, IPowerService powerService)
    {
        _logger = logger;
        _powerService = powerService;
    }

    public async Task<IEnumerable<OutputItemDto>> GetForcastAsync(DateTime requestedUtc)
    {
        var rows = new List<OutputItemDto>();
        var trades = await GetTradesFromExternalServiceAsync(requestedUtc);
        if (!trades.Any())
        {
            _logger.LogWarning("No trades retrieved for date {RequestedDate}", requestedUtc);
            return rows;
        }

        var periodsCount = trades.Min(t => t.Periods.Length);
        _logger.LogInformation("Periods count: {PeriodsCount} for date {RequestedDate}", periodsCount, requestedUtc);

        for (int i = 0; i < periodsCount; i++)
        {
            var calculatedVolume = trades.Sum(t => t.Periods[i].Volume);
            var periodCounter = i + 1;
            var auxDate = new DateTime(requestedUtc.Year, requestedUtc.Month, requestedUtc.Day, requestedUtc.Hour, 0, 0);
            var periodAsDateTime = auxDate.AddHours(periodCounter);
            rows.Add(new OutputItemDto { DateTime = periodAsDateTime, Volume = calculatedVolume });
        }

        _logger.LogInformation("Rows {RowsCount} generated for {RequestedDate}", rows.Count, requestedUtc);

        return rows;
    }

    internal async Task<IEnumerable<PowerTrade>> GetTradesFromExternalServiceAsync(DateTime requestedUtc)
    {
        //TODO 
        //Read from config
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
}
