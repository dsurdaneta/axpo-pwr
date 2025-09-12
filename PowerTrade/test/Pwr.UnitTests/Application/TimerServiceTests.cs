using Microsoft.Extensions.Logging.Abstractions;
using Pwr.Application.Services;
using Shouldly;

namespace Pwr.UnitTests.Application;

public class TimerServiceTests
{
    private readonly TimerService _timerService = new(NullLogger<TimerService>.Instance);

    [Fact]
    public void Start_TimerStartsSuccessfully()
    {
        // Arrange
        var callbackInvoked = false;
        Func<Task> callback = () =>
        {
            callbackInvoked = true;
            return Task.CompletedTask;
        };

        // Act
        _timerService.Start(100, callback);
        Task.Delay(200).Wait(); // Wait to allow timer to trigger

        // Assert
        _timerService.Stop();
        callbackInvoked.ShouldBeTrue();
    }
}
