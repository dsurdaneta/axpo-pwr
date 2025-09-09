using Axpo;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pwr.App.Services;

public class ImportService
{
    private readonly ILogger<ImportService> _logger;
    private readonly IPowerService _powerService;

    public ImportService(ILogger<ImportService> logger, PowerService powerService)
    {
        _logger = logger;
        _powerService = powerService;
    }

    public async Task<IEnumerable<PowerTrade>> GetForcastTradesFromExternalService()
    {
        //TODO 
        //Read from config
        //implement interface
        //DST validation
        var requestedUtc = DateTime.UtcNow.AddDays(1); // should request the forcast for the next day
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
