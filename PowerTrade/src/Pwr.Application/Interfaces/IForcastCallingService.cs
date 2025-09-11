using Pwr.Domain.Models;

namespace Pwr.Application.Interfaces;

public interface IForcastCallingService
{
    Task<List<InputItemDto>> GetForcastAsync(DateTime requestedUtc);
}
