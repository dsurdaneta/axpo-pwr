using Axpo;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Pwr.Infrastructure.Services;
using Shouldly;

namespace Pwr.UnitTests.Infrastructure;

public class ForcastCallingServiceTests
{
    private readonly IPowerService _powerServiceMock;
    private readonly ForcastCallingService _service;

    public ForcastCallingServiceTests()
    {
        var loggerMock = NullLogger<ForcastCallingService>.Instance;
        _powerServiceMock = Substitute.For<IPowerService>();
        _service = new ForcastCallingService(loggerMock, _powerServiceMock);
    }

    [Fact]
    public async Task GetForcastAsync_ReturnsAggregatedVolumes()
    {
        // Arrange
        var requestedDate = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc);        
        var trade1 = PowerTrade.Create(requestedDate, 2);
        var trade2 = PowerTrade.Create(requestedDate, 2);
        var trades = new List<PowerTrade> { trade1, trade2 };

        _powerServiceMock.GetTradesAsync(requestedDate).Returns(trades);

        // Act
        var result = await _service.GetForcastAsync(requestedDate);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
        result[0].Volume.ShouldBe(trade1.Periods[0].Volume + trade2.Periods[0].Volume);
        result[0].DateTime.ShouldBe(new DateTime(2024, 6, 1, 1, 0, 0));
        result[1].Volume.ShouldBe(trade1.Periods[1].Volume + trade2.Periods[1].Volume);
        result[1].DateTime.ShouldBe(new DateTime(2024, 6, 1, 2, 0, 0));
    }

    [Fact]
    public async Task GetForcastAsync_NoTrades_ReturnsEmptyList()
    {
        // Arrange
        var requestedDate = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        _powerServiceMock.GetTradesAsync(requestedDate).Returns(new List<PowerTrade>());

        // Act
        var result = await _service.GetForcastAsync(requestedDate);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(0);
    }

    [Fact]
    public async Task GetTradesFromExternalServiceAsync_HandlesException()
    {
        // Arrange
        var requestedDate = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        _powerServiceMock.GetTradesAsync(requestedDate).Throws(new PowerServiceException("Service error"));

        // Act
        var result = await _service.GetTradesFromExternalServiceAsync(requestedDate);

        // Assert
        result.ShouldNotBeNull();
        result.Count().ShouldBe(0);
    }
}
