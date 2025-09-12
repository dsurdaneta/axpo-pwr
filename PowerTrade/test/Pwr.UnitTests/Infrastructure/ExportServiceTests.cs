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
    [Fact]
    public void GenerateReport_WhenRowsAreEmpty_ShouldReturnFalse()
    {
        // Arrange
        var loggerMock = NullLogger<ExportService>.Instance;
        IOptionsMonitor<CsvOptions> optionsMock = Substitute.For<IOptionsMonitor<CsvOptions>>();
        optionsMock.CurrentValue.Returns(new CsvOptions { FolderPath = "SomePath" });

        var exportService = new ExportService(loggerMock, optionsMock);
        var requestedUtc = DateTime.UtcNow;
        var emptyRows = new List<InputItemDto>();

        // Act
        var result = exportService.GenerateReport(requestedUtc, emptyRows);

        // Assert
        result.ShouldBeFalse();
    }
}
