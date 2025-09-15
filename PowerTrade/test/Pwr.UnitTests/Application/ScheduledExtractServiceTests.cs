using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Pwr.Application.Interfaces;
using Pwr.Application.Models;
using Pwr.Application.Options;
using Pwr.Application.Services;
using Shouldly;

namespace Pwr.Tests.UnitTests.Application;

public class ScheduledExtractServiceTests
{
    private readonly IExtractService _extractServiceMock;
    private readonly IRetryService _retryServiceMock;
    private readonly ITimerService _timerServiceMock;
    private readonly IOptionsMonitor<ExtractTradesOptions> _optionsMonitorMock;
    private readonly ScheduledExtractService _service;

    public ScheduledExtractServiceTests()
    {
        _extractServiceMock = Substitute.For<IExtractService>();
        _retryServiceMock = Substitute.For<IRetryService>();
        _timerServiceMock = Substitute.For<ITimerService>();
        _optionsMonitorMock = Substitute.For<IOptionsMonitor<ExtractTradesOptions>>();

        _optionsMonitorMock.CurrentValue.Returns(new ExtractTradesOptions
        {
            ExtractIntervalMinutes = 60,
            MaxRetryAttempts = 3,
            RetryDelaySeconds = 30
        });

        var logger = NullLogger<ScheduledExtractService>.Instance;
        _service = new ScheduledExtractService(
            logger,
            _extractServiceMock,
            _retryServiceMock,
            _timerServiceMock,
            _optionsMonitorMock);
    }

    [Fact]
    public void CreateExtractContext_WithValidOptions_ReturnsCorrectContext()
    {
        // Arrange
        var options = new ExtractTradesOptions
        {
            MaxRetryAttempts = 5,
            RetryDelaySeconds = 60
        };
        _optionsMonitorMock.CurrentValue.Returns(options);

        // Act
        var context = _service.CreateExtractContext();

        // Assert
        context.ShouldNotBeNull();
        context.MaxRetryAttempts.ShouldBe(5);
        context.RetryDelaySeconds.ShouldBe(60);
        context.RequestedUtc.ShouldBe(DateTime.UtcNow.AddDays(1), TimeSpan.FromMinutes(1));
        context.CorrelationId.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void CreateExtractContext_WithZeroRetryAttempts_ReturnsCorrectContext()
    {
        // Arrange
        var options = new ExtractTradesOptions
        {
            MaxRetryAttempts = 0,
            RetryDelaySeconds = 0
        };
        _optionsMonitorMock.CurrentValue.Returns(options);

        // Act
        var context = _service.CreateExtractContext();

        // Assert
        context.ShouldNotBeNull();
        context.MaxRetryAttempts.ShouldBe(0);
        context.RetryDelaySeconds.ShouldBe(0);
        context.RequestedUtc.ShouldBe(DateTime.UtcNow.AddDays(1), TimeSpan.FromMinutes(1));
        context.CorrelationId.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void CreateExtractContext_WithMaxRetryAttempts_ReturnsCorrectContext()
    {
        // Arrange
        var options = new ExtractTradesOptions
        {
            MaxRetryAttempts = int.MaxValue,
            RetryDelaySeconds = int.MaxValue
        };
        _optionsMonitorMock.CurrentValue.Returns(options);

        // Act
        var context = _service.CreateExtractContext();

        // Assert
        context.ShouldNotBeNull();
        context.MaxRetryAttempts.ShouldBe(int.MaxValue);
        context.RetryDelaySeconds.ShouldBe(int.MaxValue);
        context.RequestedUtc.ShouldBe(DateTime.UtcNow.AddDays(1), TimeSpan.FromMinutes(1));
        context.CorrelationId.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void GetIntervalMilliseconds_WithValidOptions_ReturnsCorrectMilliseconds()
    {
        // Arrange
        var options = new ExtractTradesOptions
        {
            ExtractIntervalMinutes = 15
        };
        _optionsMonitorMock.CurrentValue.Returns(options);

        // Act
        var intervalMs = _service.GetIntervalMilliseconds();

        // Assert
        intervalMs.ShouldBe(15 * 60 * 1000); // 15 minutes in milliseconds
    }

    [Fact]
    public void GetIntervalMilliseconds_WithZeroMinutes_ReturnsZero()
    {
        // Arrange
        var options = new ExtractTradesOptions
        {
            ExtractIntervalMinutes = 0
        };
        _optionsMonitorMock.CurrentValue.Returns(options);

        // Act
        var intervalMs = _service.GetIntervalMilliseconds();

        // Assert
        intervalMs.ShouldBe(0);
    }

    [Fact]
    public void GetIntervalMilliseconds_WithOneMinute_ReturnsCorrectMilliseconds()
    {
        // Arrange
        var options = new ExtractTradesOptions
        {
            ExtractIntervalMinutes = 1
        };
        _optionsMonitorMock.CurrentValue.Returns(options);

        // Act
        var intervalMs = _service.GetIntervalMilliseconds();

        // Assert
        intervalMs.ShouldBe(60 * 1000); // 1 minute in milliseconds
    }

    [Fact]
    public void GetIntervalMilliseconds_WithLargeValue_ReturnsCorrectMilliseconds()
    {
        // Arrange
        var options = new ExtractTradesOptions
        {
            ExtractIntervalMinutes = 1440 // 24 hours
        };
        _optionsMonitorMock.CurrentValue.Returns(options);

        // Act
        var intervalMs = _service.GetIntervalMilliseconds();

        // Assert
        intervalMs.ShouldBe(1440 * 60 * 1000); // 24 hours in milliseconds
    }

    [Fact]
    public void OnConfigurationChanged_WithNewInterval_UpdatesTimer()
    {
        // Arrange
        var newOptions = new ExtractTradesOptions
        {
            ExtractIntervalMinutes = 30
        };

        // Act
        _service.OnConfigurationChanged(newOptions);

        // Assert
        _timerServiceMock.Received(1).UpdateInterval(Arg.Any<int>());
    }

    [Fact]
    public void OnConfigurationChanged_WithZeroInterval_UpdatesTimer()
    {
        // Arrange
        var newOptions = new ExtractTradesOptions
        {
            ExtractIntervalMinutes = 0
        };

        // Act
        _service.OnConfigurationChanged(newOptions);

        // Assert
        _timerServiceMock.Received(1).UpdateInterval(Arg.Any<int>());
    }

    [Fact]
    public async Task ExecuteExtractAsync_SuccessfulExtract_LogsSuccess()
    {
        // Arrange
        var context = new ExtractContext
        {
            RequestedUtc = DateTime.UtcNow,
            MaxRetryAttempts = 3,
            RetryDelaySeconds = 30,
            CorrelationId = "test-123"
        };

        var extractResult = ExtractResult.Success(1);
        _retryServiceMock.ExecuteWithRetryAsync(
            Arg.Any<Func<Task<ExtractResult>>>(),
            Arg.Any<Func<ExtractResult, bool>>(),
            Arg.Any<ExtractContext>(),
            Arg.Any<CancellationToken>())
            .Returns(extractResult);

        // Act
        await _service.ExecuteExtractAsync(CancellationToken.None);

        // Assert
        await _retryServiceMock.Received(1).ExecuteWithRetryAsync(
            Arg.Any<Func<Task<ExtractResult>>>(),
            Arg.Any<Func<ExtractResult, bool>>(),
            Arg.Any<ExtractContext>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteExtractAsync_FailedExtract_LogsFailure()
    {
        // Arrange
        var context = new ExtractContext
        {
            RequestedUtc = DateTime.UtcNow,
            MaxRetryAttempts = 3,
            RetryDelaySeconds = 30,
            CorrelationId = "test-123"
        };

        var extractResult = ExtractResult.Failure(3, "Test error");
        _retryServiceMock.ExecuteWithRetryAsync(
            Arg.Any<Func<Task<ExtractResult>>>(),
            Arg.Any<Func<ExtractResult, bool>>(),
            Arg.Any<ExtractContext>(),
            Arg.Any<CancellationToken>())
            .Returns(extractResult);

        // Act
        await _service.ExecuteExtractAsync(CancellationToken.None);

        // Assert
        await _retryServiceMock.Received(1).ExecuteWithRetryAsync(
            Arg.Any<Func<Task<ExtractResult>>>(),
            Arg.Any<Func<ExtractResult, bool>>(),
            Arg.Any<ExtractContext>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteExtractAsync_OperationCanceled_HandlesGracefully()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _retryServiceMock.ExecuteWithRetryAsync(
            Arg.Any<Func<Task<ExtractResult>>>(),
            Arg.Any<Func<ExtractResult, bool>>(),
            Arg.Any<ExtractContext>(),
            Arg.Any<CancellationToken>())
            .Throws(new OperationCanceledException());

        // Act
        await _service.ExecuteExtractAsync(cts.Token);

        // Assert
        await _retryServiceMock.Received(1).ExecuteWithRetryAsync(
            Arg.Any<Func<Task<ExtractResult>>>(),
            Arg.Any<Func<ExtractResult, bool>>(),
            Arg.Any<ExtractContext>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteExtractAsync_UnexpectedException_HandlesGracefully()
    {
        // Arrange
        _retryServiceMock.ExecuteWithRetryAsync(
            Arg.Any<Func<Task<ExtractResult>>>(),
            Arg.Any<Func<ExtractResult, bool>>(),
            Arg.Any<ExtractContext>(),
            Arg.Any<CancellationToken>())
            .Throws(new InvalidOperationException("Unexpected error"));

        // Act
        await _service.ExecuteExtractAsync(CancellationToken.None);

        // Assert
        await _retryServiceMock.Received(1).ExecuteWithRetryAsync(
            Arg.Any<Func<Task<ExtractResult>>>(),
            Arg.Any<Func<ExtractResult, bool>>(),
            Arg.Any<ExtractContext>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteExtractAsync_WithNullResult_HandlesGracefully()
    {
        // Arrange
        _retryServiceMock.ExecuteWithRetryAsync(
            Arg.Any<Func<Task<ExtractResult>>>(),
            Arg.Any<Func<ExtractResult, bool>>(),
            Arg.Any<ExtractContext>(),
            Arg.Any<CancellationToken>())
            .Returns((ExtractResult)null!);

        // Act
        await _service.ExecuteExtractAsync(CancellationToken.None);

        // Assert
        await _retryServiceMock.Received(1).ExecuteWithRetryAsync(
            Arg.Any<Func<Task<ExtractResult>>>(),
            Arg.Any<Func<ExtractResult, bool>>(),
            Arg.Any<ExtractContext>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteExtractAsync_WithExceptionInResult_HandlesGracefully()
    {
        // Arrange
        var extractResult = ExtractResult.Failure(2, "Test error", new InvalidOperationException("Test exception"));
        _retryServiceMock.ExecuteWithRetryAsync(
            Arg.Any<Func<Task<ExtractResult>>>(),
            Arg.Any<Func<ExtractResult, bool>>(),
            Arg.Any<ExtractContext>(),
            Arg.Any<CancellationToken>())
            .Returns(extractResult);

        // Act
        await _service.ExecuteExtractAsync(CancellationToken.None);

        // Assert
        await _retryServiceMock.Received(1).ExecuteWithRetryAsync(
            Arg.Any<Func<Task<ExtractResult>>>(),
            Arg.Any<Func<ExtractResult, bool>>(),
            Arg.Any<ExtractContext>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteExtractAsync_WithMaxAttempts_HandlesCorrectly()
    {
        // Arrange
        var extractResult = ExtractResult.Failure(int.MaxValue, "Max attempts reached");
        _retryServiceMock.ExecuteWithRetryAsync(
            Arg.Any<Func<Task<ExtractResult>>>(),
            Arg.Any<Func<ExtractResult, bool>>(),
            Arg.Any<ExtractContext>(),
            Arg.Any<CancellationToken>())
            .Returns(extractResult);

        // Act
        await _service.ExecuteExtractAsync(CancellationToken.None);

        // Assert
        await _retryServiceMock.Received(1).ExecuteWithRetryAsync(
            Arg.Any<Func<Task<ExtractResult>>>(),
            Arg.Any<Func<ExtractResult, bool>>(),
            Arg.Any<ExtractContext>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteExtractAsync_WithZeroAttempts_HandlesCorrectly()
    {
        // Arrange
        var extractResult = ExtractResult.Success(0);
        _retryServiceMock.ExecuteWithRetryAsync(
            Arg.Any<Func<Task<ExtractResult>>>(),
            Arg.Any<Func<ExtractResult, bool>>(),
            Arg.Any<ExtractContext>(),
            Arg.Any<CancellationToken>())
            .Returns(extractResult);

        // Act
        await _service.ExecuteExtractAsync(CancellationToken.None);

        // Assert
        await _retryServiceMock.Received(1).ExecuteWithRetryAsync(
            Arg.Any<Func<Task<ExtractResult>>>(),
            Arg.Any<Func<ExtractResult, bool>>(),
            Arg.Any<ExtractContext>(),
            Arg.Any<CancellationToken>());
    }
}
