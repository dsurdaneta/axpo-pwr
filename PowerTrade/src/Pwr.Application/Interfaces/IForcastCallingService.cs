using Pwr.Domain.Models;

namespace Pwr.Application.Interfaces;

public interface IForcastCallingService
{
    Task<IEnumerable<OutputItemDto>> GetForcastAsync(DateTime requestedUtc);
}
