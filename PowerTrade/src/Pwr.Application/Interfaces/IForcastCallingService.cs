using Pwr.Domain.Models;

namespace Pwr.Application.Interfaces;

public interface IForcastCallingService
{
    Task<List<OutputItemDto>> GetForcastAsync(DateTime requestedUtc);
}
