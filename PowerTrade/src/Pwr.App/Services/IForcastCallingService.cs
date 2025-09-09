using Pwr.App.Models;

namespace Pwr.App.Services;

public interface IForcastCallingService
{
    Task<List<OutputItemDto>> GetForcastAsync(DateTime requestedUtc);
}
