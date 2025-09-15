using Pwr.Application.Models;
using Shouldly;

namespace Pwr.Tests.UnitTests.Application.Models;

public class ExtractContextTests
{
    [Fact]
    public void ExtractContext_DefaultConstructor_ShouldInitializeCorrectly()
    {
        // Act
        var context = new ExtractContext();

        // Assert
        context.RequestedUtc.ShouldBe(default);
        context.MaxRetryAttempts.ShouldBe(0);
        context.RetryDelaySeconds.ShouldBe(0);
        context.CorrelationId.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void ExtractContext_WithValues_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var requestedUtc = new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Utc);
        var maxRetryAttempts = 5;
        var retryDelaySeconds = 30;
        var correlationId = "test-correlation-123";

        // Act
        var context = new ExtractContext
        {
            RequestedUtc = requestedUtc,
            MaxRetryAttempts = maxRetryAttempts,
            RetryDelaySeconds = retryDelaySeconds,
            CorrelationId = correlationId
        };

        // Assert
        context.RequestedUtc.ShouldBe(requestedUtc);
        context.MaxRetryAttempts.ShouldBe(maxRetryAttempts);
        context.RetryDelaySeconds.ShouldBe(retryDelaySeconds);
        context.CorrelationId.ShouldBe(correlationId);
    }

    [Fact]
    public void ExtractContext_CorrelationId_ShouldBeGeneratedAutomatically()
    {
        // Act
        var context1 = new ExtractContext();
        var context2 = new ExtractContext();

        // Assert
        context1.CorrelationId.ShouldNotBeNullOrEmpty();
        context2.CorrelationId.ShouldNotBeNullOrEmpty();
        context1.CorrelationId.ShouldNotBe(context2.CorrelationId);
    }

    [Fact]
    public void ExtractContext_CorrelationId_ShouldBeShortFormat()
    {
        // Act
        var context = new ExtractContext();

        // Assert
        context.CorrelationId.Length.ShouldBe(8);
        context.CorrelationId.ShouldMatch("^[a-f0-9]{8}$");
    }

    [Fact]
    public void ExtractContext_WithUtcDateTime_ShouldPreserveKind()
    {
        // Arrange
        var utcDateTime = new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Utc);

        // Act
        var context = new ExtractContext
        {
            RequestedUtc = utcDateTime
        };

        // Assert
        context.RequestedUtc.Kind.ShouldBe(DateTimeKind.Utc);
        context.RequestedUtc.ShouldBe(utcDateTime);
    }

    [Fact]
    public void ExtractContext_WithZeroRetryAttempts_ShouldAcceptZero()
    {
        // Act
        var context = new ExtractContext
        {
            MaxRetryAttempts = 0,
            RetryDelaySeconds = 0
        };

        // Assert
        context.MaxRetryAttempts.ShouldBe(0);
        context.RetryDelaySeconds.ShouldBe(0);
    }

    [Fact]
    public void ExtractContext_WithNegativeValues_ShouldAcceptNegative()
    {
        // Act
        var context = new ExtractContext
        {
            MaxRetryAttempts = -1,
            RetryDelaySeconds = -5
        };

        // Assert
        context.MaxRetryAttempts.ShouldBe(-1);
        context.RetryDelaySeconds.ShouldBe(-5);
    }

    [Fact]
    public void ExtractContext_WithMaxValues_ShouldAcceptMaxValues()
    {
        // Act
        var context = new ExtractContext
        {
            MaxRetryAttempts = int.MaxValue,
            RetryDelaySeconds = int.MaxValue
        };

        // Assert
        context.MaxRetryAttempts.ShouldBe(int.MaxValue);
        context.RetryDelaySeconds.ShouldBe(int.MaxValue);
    }

    [Fact]
    public void ExtractContext_WithMinValues_ShouldAcceptMinValues()
    {
        // Act
        var context = new ExtractContext
        {
            MaxRetryAttempts = int.MinValue,
            RetryDelaySeconds = int.MinValue
        };

        // Assert
        context.MaxRetryAttempts.ShouldBe(int.MinValue);
        context.RetryDelaySeconds.ShouldBe(int.MinValue);
    }

    [Fact]
    public void ExtractContext_WithDifferentDateTimeKinds_ShouldPreserveKind()
    {
        // Arrange
        var testCases = new[]
        {
            new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Utc),
            new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Local),
            new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Unspecified)
        };

        foreach (var dateTime in testCases)
        {
            // Act
            var context = new ExtractContext
            {
                RequestedUtc = dateTime
            };

            // Assert
            context.RequestedUtc.Kind.ShouldBe(dateTime.Kind);
            context.RequestedUtc.ShouldBe(dateTime);
        }
    }
}
