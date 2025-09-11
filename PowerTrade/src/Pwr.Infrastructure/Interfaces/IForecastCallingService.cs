using Pwr.Domain.Models;

namespace Pwr.Infrastructure.Interfaces;

public interface IForecastCallingService
{
    Task<List<InputItemDto>> GetForecastAsync(DateTime requestedUtc);
}
