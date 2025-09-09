using Axpo;
using Microsoft.Extensions.Logging;

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
}
