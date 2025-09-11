using Pwr.Domain.Models;

namespace Pwr.Infrastructure.Interfaces;

/// <summary>
/// Handles communication with the external power trade forecasting service.
/// </summary>
public interface IForecastCallingService
{
    /// <summary>
    /// Fetches the power trade forecast for the specified UTC date.
    /// </summary>
    /// <param name="requestedUtc">specified UTC date</param>
    /// <returns>the list of power volumes retrieved from the external service</returns>
    Task<List<InputItemDto>> GetForecastAsync(DateTime requestedUtc);
}
