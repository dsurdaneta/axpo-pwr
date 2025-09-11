namespace Pwr.Application.Interfaces;

public interface IScheduledExtractService
{
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
}
