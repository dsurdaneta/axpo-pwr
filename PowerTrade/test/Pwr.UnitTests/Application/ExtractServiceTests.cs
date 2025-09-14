using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Pwr.Application.Models;
using Pwr.Application.Services;
using Pwr.Domain.Models;
using Pwr.Infrastructure.Interfaces;
using Shouldly;

namespace Pwr.UnitTests.Application;

public class ExtractServiceTests
{
    private readonly IForecastCallingService _forcastCallingServiceMock;
    private readonly IExportService _exportServiceMock;
    private readonly ExtractService _extractService;

    public ExtractServiceTests()
    {
        _forcastCallingServiceMock = Substitute.For<IForecastCallingService>();
        _exportServiceMock = Substitute.For<IExportService>();
        var loggerMock = NullLogger<ExtractService>.Instance;
        _extractService = new ExtractService(loggerMock, _forcastCallingServiceMock, _exportServiceMock);
    }

    [Fact]
    public async Task PerformExtractAsync_SuccessfulExtract_ReturnsSuccess()
    {
        // Arrange
        var context = new ExtractContext
        {
            RequestedUtc = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc),
        };

        var forecastData = new List<InputItemDto>
        {
            new() 
            {
                DateTime = new DateTime(2024, 6, 1, 1, 0, 0),
                Volume = 5
            }
        };

        _forcastCallingServiceMock.GetForecastAsync(context.RequestedUtc).Returns(forecastData);
        _exportServiceMock.GenerateReport(context.RequestedUtc, forecastData).Returns("SomeFileName");

        // Act
        var result = await _extractService.PerformExtractAsync(context);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.AttemptsMade.ShouldBeGreaterThanOrEqualTo(1);
        result.ErrorMessage.ShouldBeNull();
    }

    [Fact]
    public async Task PerformExtractAsync_NoForecastData_ReturnsFailure()
    {
        // Arrange
        var context = new ExtractContext
        {
            RequestedUtc = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc),
        };

        _forcastCallingServiceMock.GetForecastAsync(context.RequestedUtc).Returns(new List<InputItemDto>());

        // Act
        var result = await _extractService.PerformExtractAsync(context);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.AttemptsMade.ShouldBeGreaterThanOrEqualTo(1);
        result.ErrorMessage.ShouldBe("No forecast data available");
    }

    [Fact]
    public async Task PerformExtractAsync_ReportGenerationFails_ReturnsFailure()
    {
        // Arrange
        var context = new ExtractContext
        {
            RequestedUtc = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc),
        };

        var forecastData = new List<InputItemDto>
        {
            new()
            {
                DateTime = new DateTime(2024, 6, 1, 1, 0, 0),
                Volume = 5
            }
        };

        _forcastCallingServiceMock.GetForecastAsync(context.RequestedUtc).Returns(forecastData);
        _exportServiceMock.GenerateReport(context.RequestedUtc, forecastData).Returns(string.Empty);

        // Act
        var result = await _extractService.PerformExtractAsync(context);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.AttemptsMade.ShouldBeGreaterThanOrEqualTo(1);
        result.ErrorMessage.ShouldBe("Failed to generate report");
    }

    [Fact]
    public async Task PerformExtractAsync_ExceptionDuringExtract_ReturnsFailure()
    {
        // Arrange
        var context = new ExtractContext
        {
            RequestedUtc = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc),
        };

        _forcastCallingServiceMock.GetForecastAsync(context.RequestedUtc)
            .Throws(new Exception("Simulated exception"));

        // Act
        var result = await _extractService.PerformExtractAsync(context);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.AttemptsMade.ShouldBeGreaterThanOrEqualTo(1);
        result.ErrorMessage.ShouldBe("Extract operation failed");
        result.Exception.ShouldNotBeNull();
        result.Exception!.Message.ShouldBe("Simulated exception");
    }
}
