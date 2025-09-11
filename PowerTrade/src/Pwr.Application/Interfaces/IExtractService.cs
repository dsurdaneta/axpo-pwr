using Pwr.Application.Models;

namespace Pwr.Application.Interfaces;

public interface IExtractService
{
    Task<ExtractResult> PerformExtractAsync(ExtractContext context, CancellationToken cancellationToken = default);
}
