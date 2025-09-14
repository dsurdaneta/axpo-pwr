using Pwr.Application.Models;
using Shouldly;

namespace Pwr.UnitTests.Application.Models;

public class ExtractResultTests
{
    [Fact]
    public void Success_WithZeroAttempts_ShouldCreateSuccessResult()
    {
        // Arrange
        var attemptsMade = 0;

        // Act
        var result = ExtractResult.Success(attemptsMade);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.AttemptsMade.ShouldBe(0);
        result.ErrorMessage.ShouldBeNull();
        result.Exception.ShouldBeNull();
    }

    [Fact]
    public void Success_WithNegativeAttempts_ShouldCreateSuccessResult()
    {
        // Arrange
        var attemptsMade = -1;

        // Act
        var result = ExtractResult.Success(attemptsMade);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.AttemptsMade.ShouldBe(-1);
        result.ErrorMessage.ShouldBeNull();
        result.Exception.ShouldBeNull();
    }

    [Fact]
    public void Failure_WithErrorMessage_ShouldCreateFailureResult()
    {
        // Arrange
        var attemptsMade = 3;
        var errorMessage = "Test error message";

        // Act
        var result = ExtractResult.Failure(attemptsMade, errorMessage);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.AttemptsMade.ShouldBe(attemptsMade);
        result.ErrorMessage.ShouldBe(errorMessage);
        result.Exception.ShouldBeNull();
    }

    [Fact]
    public void Failure_WithErrorMessageAndException_ShouldCreateFailureResult()
    {
        // Arrange
        var attemptsMade = 3;
        var errorMessage = "Test error message";
        var exception = new InvalidOperationException("Test exception");

        // Act
        var result = ExtractResult.Failure(attemptsMade, errorMessage, exception);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.AttemptsMade.ShouldBe(attemptsMade);
        result.ErrorMessage.ShouldBe(errorMessage);
        result.Exception.ShouldBe(exception);
    }

    [Fact]
    public void Failure_WithNullErrorMessage_ShouldCreateFailureResult()
    {
        // Arrange
        var attemptsMade = 3;
        string? errorMessage = null;

        // Act
        var result = ExtractResult.Failure(attemptsMade, errorMessage!);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.AttemptsMade.ShouldBe(attemptsMade);
        result.ErrorMessage.ShouldBeNull();
        result.Exception.ShouldBeNull();
    }

    [Fact]
    public void Failure_WithEmptyErrorMessage_ShouldCreateFailureResult()
    {
        // Arrange
        var attemptsMade = 3;
        var errorMessage = string.Empty;

        // Act
        var result = ExtractResult.Failure(attemptsMade, errorMessage);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.AttemptsMade.ShouldBe(attemptsMade);
        result.ErrorMessage.ShouldBe(string.Empty);
        result.Exception.ShouldBeNull();
    }

    [Fact]
    public void Failure_WithNullException_ShouldCreateFailureResult()
    {
        // Arrange
        var attemptsMade = 3;
        var errorMessage = "Test error message";
        Exception? exception = null;

        // Act
        var result = ExtractResult.Failure(attemptsMade, errorMessage, exception);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.AttemptsMade.ShouldBe(attemptsMade);
        result.ErrorMessage.ShouldBe(errorMessage);
        result.Exception.ShouldBeNull();
    }

    [Fact]
    public void Failure_WithZeroAttempts_ShouldCreateFailureResult()
    {
        // Arrange
        var attemptsMade = 0;
        var errorMessage = "Test error message";

        // Act
        var result = ExtractResult.Failure(attemptsMade, errorMessage);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.AttemptsMade.ShouldBe(0);
        result.ErrorMessage.ShouldBe(errorMessage);
        result.Exception.ShouldBeNull();
    }

    [Fact]
    public void Failure_WithNegativeAttempts_ShouldCreateFailureResult()
    {
        // Arrange
        var attemptsMade = -1;
        var errorMessage = "Test error message";

        // Act
        var result = ExtractResult.Failure(attemptsMade, errorMessage);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.AttemptsMade.ShouldBe(-1);
        result.ErrorMessage.ShouldBe(errorMessage);
        result.Exception.ShouldBeNull();
    }

    [Fact]
    public void Success_CompletedAt_ShouldBeSetToCurrentTime()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;
        var attemptsMade = 1;

        // Act
        var result = ExtractResult.Success(attemptsMade);
        var afterCreation = DateTime.UtcNow;

        // Assert
        result.CompletedAt.ShouldBeGreaterThanOrEqualTo(beforeCreation);
        result.CompletedAt.ShouldBeLessThanOrEqualTo(afterCreation);
    }

    [Fact]
    public void Failure_CompletedAt_ShouldBeSetToCurrentTime()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;
        var attemptsMade = 1;
        var errorMessage = "Test error";

        // Act
        var result = ExtractResult.Failure(attemptsMade, errorMessage);
        var afterCreation = DateTime.UtcNow;

        // Assert
        result.CompletedAt.ShouldBeGreaterThanOrEqualTo(beforeCreation);
        result.CompletedAt.ShouldBeLessThanOrEqualTo(afterCreation);
    }

    [Fact]
    public void Success_WithMaxAttempts_ShouldCreateSuccessResult()
    {
        // Arrange
        var attemptsMade = int.MaxValue;

        // Act
        var result = ExtractResult.Success(attemptsMade);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.AttemptsMade.ShouldBe(int.MaxValue);
        result.ErrorMessage.ShouldBeNull();
        result.Exception.ShouldBeNull();
    }

    [Fact]
    public void Failure_WithMaxAttempts_ShouldCreateFailureResult()
    {
        // Arrange
        var attemptsMade = int.MaxValue;
        var errorMessage = "Test error message";

        // Act
        var result = ExtractResult.Failure(attemptsMade, errorMessage);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.AttemptsMade.ShouldBe(int.MaxValue);
        result.ErrorMessage.ShouldBe(errorMessage);
        result.Exception.ShouldBeNull();
    }
}
