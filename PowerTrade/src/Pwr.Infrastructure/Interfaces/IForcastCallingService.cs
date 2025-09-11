using Pwr.Domain.Models;

namespace Pwr.Infrastructure.Interfaces;

public interface IForcastCallingService
{
    Task<List<InputItemDto>> GetForcastAsync(DateTime requestedUtc);
}
