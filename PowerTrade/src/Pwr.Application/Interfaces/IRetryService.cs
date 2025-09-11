using Pwr.Application.Models;

namespace Pwr.Application.Interfaces;

public interface IRetryService
{
    Task<ExtractResult> ExecuteWithRetryAsync<T>(
        Func<Task<T>> operation,
        Func<T, bool> successCondition,
        ExtractContext context,
        CancellationToken cancellationToken = default);
}
