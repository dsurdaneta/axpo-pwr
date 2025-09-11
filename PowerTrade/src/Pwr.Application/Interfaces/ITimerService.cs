namespace Pwr.Application.Interfaces;

public interface ITimerService : IDisposable
{
    void Start(int intervalMs, Func<Task> callback);
    void Stop();
    void UpdateInterval(int intervalMs);
}
