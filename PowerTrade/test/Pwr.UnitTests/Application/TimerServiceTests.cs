using Microsoft.Extensions.Logging.Abstractions;
using Pwr.Application.Services;
using Shouldly;

namespace Pwr.Tests.UnitTests.Application;

public class TimerServiceTests
{
    private readonly TimerService _timerService;

    public TimerServiceTests() => _timerService = new TimerService(NullLogger<TimerService>.Instance);

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

    [Fact]
    public void Start_WithZeroInterval_ShouldNotThrow()
    {
        // Arrange
        var callbackInvoked = false;
        Func<Task> callback = () =>
        {
            callbackInvoked = true;
            return Task.CompletedTask;
        };

        // Act & Assert
        Should.NotThrow(() => _timerService.Start(0, callback));
        _timerService.Stop();
    }

    [Fact]
    public void Start_WithNullCallback_ShouldNotThrow()
    {
        // Act & Assert
        Should.NotThrow(() => _timerService.Start(100, null!));
        _timerService.Stop();
    }

    [Fact]
    public void Stop_WhenNotStarted_ShouldNotThrow()
    {
        // Act & Assert
        Should.NotThrow(() => _timerService.Stop());
    }

    [Fact]
    public void UpdateInterval_WhenStarted_ShouldUpdateSuccessfully()
    {
        // Arrange
        var callbackInvoked = false;
        Func<Task> callback = () =>
        {
            callbackInvoked = true;
            return Task.CompletedTask;
        };

        // Act
        _timerService.Start(1000, callback);
        _timerService.UpdateInterval(100);
        Task.Delay(200).Wait(); // Wait to allow timer to trigger

        // Assert
        _timerService.Stop();
        callbackInvoked.ShouldBeTrue();
    }

    [Fact]
    public void UpdateInterval_WhenNotStarted_ShouldNotThrow()
    {
        // Act & Assert
        Should.NotThrow(() => _timerService.UpdateInterval(100));
    }

    [Fact]
    public void Start_MultipleTimes_ShouldReplacePreviousTimer()
    {
        // Arrange
        var firstCallbackInvoked = false;
        var secondCallbackInvoked = false;

        Func<Task> firstCallback = () =>
        {
            firstCallbackInvoked = true;
            return Task.CompletedTask;
        };

        Func<Task> secondCallback = () =>
        {
            secondCallbackInvoked = true;
            return Task.CompletedTask;
        };

        // Act
        _timerService.Start(1000, firstCallback);
        _timerService.Start(100, secondCallback);
        Task.Delay(200).Wait(); // Wait to allow timer to trigger

        // Assert
        _timerService.Stop();
        firstCallbackInvoked.ShouldBeFalse();
        secondCallbackInvoked.ShouldBeTrue();
    }

    [Fact]
    public void Dispose_ShouldDisposeResources()
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
        _timerService.Dispose();
        Task.Delay(200).Wait(); // Wait to see if callback is still invoked

        // Assert
        callbackInvoked.ShouldBeFalse();
    }

    [Fact]
    public void Start_AfterDispose_ShouldThrow()
    {
        // Arrange
        var callbackInvoked = false;
        Func<Task> callback = () =>
        {
            callbackInvoked = true;
            return Task.CompletedTask;
        };

        // Act
        _timerService.Dispose();

        // Assert
        Should.Throw<ObjectDisposedException>(() => _timerService.Start(100, callback));
    }

    [Fact]
    public void Start_CallbackThrowsException_ShouldNotCrash()
    {
        // Arrange
        var exceptionThrown = false;
        Func<Task> callback = () =>
        {
            exceptionThrown = true;
            throw new InvalidOperationException("Test exception");
        };

        // Act
        _timerService.Start(100, callback);
        Task.Delay(200).Wait(); // Wait to allow timer to trigger

        // Assert
        _timerService.Stop();
        exceptionThrown.ShouldBeTrue();
    }

    [Fact]
    public void Start_CallbackReturnsFaultedTask_ShouldNotCrash()
    {
        // Arrange
        var callbackInvoked = false;
        Func<Task> callback = () =>
        {
            callbackInvoked = true;
            return Task.FromException(new InvalidOperationException("Test exception"));
        };

        // Act
        _timerService.Start(100, callback);
        Task.Delay(200).Wait(); // Wait to allow timer to trigger

        // Assert
        _timerService.Stop();
        callbackInvoked.ShouldBeTrue();
    }
}
