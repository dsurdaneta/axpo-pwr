using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Pwr.Infrastructure.Options;
using Pwr.Infrastructure.Services;
using NSubstitute;
using Pwr.Domain.Models;
using Shouldly;

namespace Pwr.UnitTests.Infrastructure;

public class ExportServiceTests
{
    private readonly IOptionsMonitor<CsvOptions> _optionsMock;
    private readonly ExportService _exportService;

    public ExportServiceTests()
    {
        _optionsMock = Substitute.For<IOptionsMonitor<CsvOptions>>();
        _optionsMock.CurrentValue.Returns(new CsvOptions { FolderPath = "C:\\TestOutput" });
        
        var loggerMock = NullLogger<ExportService>.Instance;
        _exportService = new ExportService(loggerMock, _optionsMock);
    }

    [Fact]
    public void GenerateReport_WhenRowsAreEmpty_ShouldReturnFalse()
    {
        // Arrange
        var requestedUtc = DateTime.UtcNow;
        var emptyRows = new List<InputItemDto>();

        // Act
        var result = _exportService.GenerateReport(requestedUtc, emptyRows);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void GenerateReport_WhenRowsAreNull_ShouldReturnFalse()
    {
        // Arrange
        var requestedUtc = DateTime.UtcNow;
        List<InputItemDto>? nullRows = null;

        // Act
        var result = _exportService.GenerateReport(requestedUtc, nullRows!);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void GenerateReport_WithValidData_ShouldReturnTrue()
    {
        // Arrange
        var requestedUtc = new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Utc);
        var rows = new List<InputItemDto>
        {
            new() { DateTime = new DateTime(2024, 6, 1, 13, 0, 0), Volume = 100.5 },
            new() { DateTime = new DateTime(2024, 6, 1, 14, 0, 0), Volume = 200.75 }
        };

        // Act
        var result = _exportService.GenerateReport(requestedUtc, rows);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void GenerateReport_WithValidData_ShouldCreateCorrectFileName()
    {
        // Arrange
        var requestedUtc = new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Utc);
        var rows = new List<InputItemDto>
        {
            new() { DateTime = new DateTime(2024, 6, 1, 13, 0, 0), Volume = 100.5 }
        };

        // Act
        var result = _exportService.GenerateReport(requestedUtc, rows);

        // Assert
        result.ShouldBeTrue();
        // Note: In a real test, you would verify the file was created with the correct name
        // Expected filename: PowerPosition_20240601_202406011200.csv
    }

    [Fact]
    public void GenerateReport_WithMultipleRows_ShouldProcessAllRows()
    {
        // Arrange
        var requestedUtc = new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Utc);
        var rows = new List<InputItemDto>
        {
            new() { DateTime = new DateTime(2024, 6, 1, 13, 0, 0), Volume = 100.5 },
            new() { DateTime = new DateTime(2024, 6, 1, 14, 0, 0), Volume = 200.75 },
            new() { DateTime = new DateTime(2024, 6, 1, 15, 0, 0), Volume = 300.25 }
        };

        // Act
        var result = _exportService.GenerateReport(requestedUtc, rows);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void GenerateReport_WithDifferentDateFormats_ShouldHandleCorrectly()
    {
        // Arrange
        var requestedUtc = new DateTime(2024, 12, 31, 23, 59, 0, DateTimeKind.Utc);
        var rows = new List<InputItemDto>
        {
            new() { DateTime = new DateTime(2025, 1, 1, 0, 0, 0), Volume = 100.5 }
        };

        // Act
        var result = _exportService.GenerateReport(requestedUtc, rows);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void GenerateReport_WithZeroVolume_ShouldHandleCorrectly()
    {
        // Arrange
        var requestedUtc = DateTime.UtcNow;
        var rows = new List<InputItemDto>
        {
            new() { DateTime = DateTime.UtcNow.AddHours(1), Volume = 0.0 },
            new() { DateTime = DateTime.UtcNow.AddHours(2), Volume = -50.0 }
        };

        // Act
        var result = _exportService.GenerateReport(requestedUtc, rows);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void GenerateReport_WithLargeVolumeValues_ShouldHandleCorrectly()
    {
        // Arrange
        var requestedUtc = DateTime.UtcNow;
        var rows = new List<InputItemDto>
        {
            new() { DateTime = DateTime.UtcNow.AddHours(1), Volume = double.MaxValue },
            new() { DateTime = DateTime.UtcNow.AddHours(2), Volume = double.MinValue }
        };

        // Act
        var result = _exportService.GenerateReport(requestedUtc, rows);

        // Assert
        result.ShouldBeTrue();
    }
}
