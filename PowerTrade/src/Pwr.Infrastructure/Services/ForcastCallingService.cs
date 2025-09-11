using Axpo;
using Microsoft.Extensions.Logging;
using Pwr.Domain.Models;
using Pwr.Infrastructure.Interfaces;

[assembly: InternalsVisibleTo("Pwr.UnitTests")]
namespace Pwr.Infrastructure.Services;

public class ForcastCallingService(ILogger<ForcastCallingService> logger, IPowerService powerService) : IForcastCallingService
{
    public async Task<List<InputItemDto>> GetForcastAsync(DateTime requestedUtc)
    {
        //TODO 
        //DST validation
        var rows = new List<InputItemDto>();
        var trades = await GetTradesFromExternalServiceAsync(requestedUtc);
        if (!trades.Any())
        {
            logger.LogWarning("No trades retrieved for date {RequestedDate}", requestedUtc);
            return rows;
        }

        var periodsCount = trades.Min(t => t.Periods.Length);
        logger.LogInformation("Periods count: {PeriodsCount} for date {RequestedDate}", periodsCount, requestedUtc);

        for (int i = 0; i < periodsCount; i++)
        {
            var calculatedVolume = trades.Sum(t => t.Periods[i].Volume);
            var periodCounter = i + 1;
            var auxDate = new DateTime(requestedUtc.Year, requestedUtc.Month, requestedUtc.Day, requestedUtc.Hour, 0, 0);
            var periodAsDateTime = auxDate.AddHours(periodCounter);
            rows.Add(new InputItemDto { DateTime = periodAsDateTime, Volume = calculatedVolume });
        }

        logger.LogInformation("Rows {RowsCount} generated for {RequestedDate}", rows.Count, requestedUtc);

        return rows;
    }

    internal async Task<IEnumerable<PowerTrade>> GetTradesFromExternalServiceAsync(DateTime requestedUtc)
    {
        logger.LogInformation("Starting to get trades from external service for date {RequestedDate}", requestedUtc);

        IEnumerable<PowerTrade> trades = new List<PowerTrade>();

        try
        {
            trades = await powerService.GetTradesAsync(requestedUtc);
            logger.LogInformation("Successfully retrieved {TradeCount} trades from external service", trades.Count());
        }
        catch (Axpo.PowerServiceException ex)
        {
            logger.LogError(ex, ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while retrieving trades from external service");
        }

        return trades;
    }
}
