using Microsoft.Extensions.Logging.Abstractions;
using Pwr.Application.Models;
using Pwr.Application.Services;
using Shouldly;

namespace Pwr.Tests.UnitTests.Application;

public class RetryServiceTests
{
    private readonly RetryService _retryService;

    public RetryServiceTests() => _retryService = new RetryService(NullLogger<RetryService>.Instance);

    [Fact]
    public async Task ExecuteWithRetryAsync_SuccessfulOperation_ReturnsSuccess()
    {
        // Arrange
        var context = new ExtractContext
        {
            RequestedUtc = DateTime.UtcNow,
            MaxRetryAttempts = 3,
            RetryDelaySeconds = 1
        };

        var operation = () => Task.FromResult("success");
        var successCondition = (string result) => result == "success";

        // Act
        var result = await _retryService.ExecuteWithRetryAsync(operation, successCondition, context);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.AttemptsMade.ShouldBe(1);
        result.ErrorMessage.ShouldBeNull();
    }

    [Fact]
    public async Task ExecuteWithRetryAsync_FailingOperation_RetriesAndReturnsFailure()
    {
        // Arrange
        var context = new ExtractContext
        {
            RequestedUtc = DateTime.UtcNow,
            MaxRetryAttempts = 2,
            RetryDelaySeconds = 1
        };

        var operation = () => Task.FromResult("failure");
        var successCondition = (string result) => result == "success";

        // Act
        var result = await _retryService.ExecuteWithRetryAsync(operation, successCondition, context);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.AttemptsMade.ShouldBe(2);
        result.ErrorMessage.ShouldNotBeNull();
    }

    [Fact]
    public async Task ExecuteWithRetryAsync_SuccessOnSecondAttempt_ReturnsSuccess()
    {
        // Arrange
        var context = new ExtractContext
        {
            RequestedUtc = DateTime.UtcNow,
            MaxRetryAttempts = 3,
            RetryDelaySeconds = 1
        };

        var attemptCount = 0;
        var operation = () =>
        {
            attemptCount++;
            return Task.FromResult(attemptCount == 2 ? "success" : "failure");
        };
        var successCondition = (string result) => result == "success";

        // Act
        var result = await _retryService.ExecuteWithRetryAsync(operation, successCondition, context);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.AttemptsMade.ShouldBe(2);
        attemptCount.ShouldBe(2);
    }

    [Fact]
    public async Task ExecuteWithRetryAsync_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        var context = new ExtractContext
        {
            RequestedUtc = DateTime.UtcNow,
            MaxRetryAttempts = 5,
            RetryDelaySeconds = 10
        };

        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(100));

        var operation = () => Task.FromResult("failure");
        var successCondition = (string result) => result == "success";

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(async () =>
            await _retryService.ExecuteWithRetryAsync(operation, successCondition, context, cts.Token));
    }
}
