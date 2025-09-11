using Axpo;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Pwr.Domain.Models;
using Pwr.Infrastructure.Services;
using Shouldly;

namespace Pwr.UnitTests.Infrastructure;

public class ForecastCallingServiceTests
{
    private readonly IPowerService _powerServiceMock;
    private readonly ForecastCallingService _service;

    public ForecastCallingServiceTests()
    {
        var loggerMock = NullLogger<ForecastCallingService>.Instance;
        _powerServiceMock = Substitute.For<IPowerService>();
        _service = new ForecastCallingService(loggerMock, _powerServiceMock);
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
        var result = await _service.GetForecastAsync(requestedDate);

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
        _powerServiceMock.GetTradesAsync(requestedDate).Returns([]);

        // Act
        var result = await _service.GetForecastAsync(requestedDate);

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

    [Fact]
    public async Task GetForcastAsync_DuringDSTSpringForward_GeneratesValidTimes()
    {
        // Arrange
        // DST spring forward in Europe (last Sunday in March 2024)
        // Clocks spring forward from 2:00 AM to 3:00 AM, so 2:00 AM doesn't exist
        var requestedDate = new DateTime(2024, 3, 31, 0, 0, 0, DateTimeKind.Utc);
        PowerTrade trade = GetFullDayTrades(requestedDate);
        _powerServiceMock.GetTradesAsync(requestedDate).Returns([trade]);

        // Act
        var result = await _service.GetForecastAsync(requestedDate);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(24);

        // Verify all times are valid UTC times (no DST issues)
        ValidateResultDateTimes(requestedDate, result);
    }

    [Fact]
    public async Task GetForcastAsync_DuringDSTFallBack_GeneratesValidTimes()
    {
        // Arrange
        // DST fall back in Europe (last Sunday in October 2024)
        // Clocks fall back from 3:00 AM to 2:00 AM, so there are two 2:00 AMs
        var requestedDate = new DateTime(2024, 10, 27, 0, 0, 0, DateTimeKind.Utc);        
        var trade = GetFullDayTrades(requestedDate);
        _powerServiceMock.GetTradesAsync(requestedDate).Returns([trade]);

        // Act
        var result = await _service.GetForecastAsync(requestedDate);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(24);

        // Verify all times are valid UTC times (no DST issues)
        ValidateResultDateTimes(requestedDate, result);
    }

    [Fact]
    public async Task GetForcastAsync_UTCOnly_NoDSTIssues()
    {
        // Arrange
        // Test that working in UTC eliminates DST concerns on a known DST transition date
        var requestedDate = new DateTime(2024, 3, 31, 12, 0, 0, DateTimeKind.Utc);
        var trade = PowerTrade.Create(requestedDate, 4);
        _powerServiceMock.GetTradesAsync(requestedDate).Returns([trade]);

        // Act
        var result = await _service.GetForecastAsync(requestedDate);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(4);
        
        // All times should be consecutive UTC hours
        result[0].DateTime.ShouldBe(new DateTime(2024, 3, 31, 13, 0, 0, DateTimeKind.Utc));
        result[1].DateTime.ShouldBe(new DateTime(2024, 3, 31, 14, 0, 0, DateTimeKind.Utc));
        result[2].DateTime.ShouldBe(new DateTime(2024, 3, 31, 15, 0, 0, DateTimeKind.Utc));
        result[3].DateTime.ShouldBe(new DateTime(2024, 3, 31, 16, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public async Task GetForcastAsync_LeapYearDST_HandlesCorrectly()
    {
        // Arrange
        // Test DST transition in a leap year
        var requestedDate = new DateTime(2024, 2, 29, 0, 0, 0, DateTimeKind.Utc);
        var trade = PowerTrade.Create(requestedDate, 12);
        _powerServiceMock.GetTradesAsync(requestedDate).Returns([trade]);

        // Act
        var result = await _service.GetForecastAsync(requestedDate);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(12);

        // Verify times are correctly calculated even on leap day
        ValidateResultDateTimes(requestedDate, result);
    }

    [Fact]
    public async Task GetForcastAsync_YearBoundaryDST_HandlesCorrectly()
    {
        // Arrange
        // Test DST transition at year boundary (New Year's Eve)
        var requestedDate = new DateTime(2024, 12, 31, 22, 0, 0, DateTimeKind.Utc);
        var trade = PowerTrade.Create(requestedDate, 4);
        _powerServiceMock.GetTradesAsync(requestedDate).Returns([trade]);

        // Act
        var result = await _service.GetForecastAsync(requestedDate);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(4);
        
        // Verify times cross year boundary correctly
        result[0].DateTime.ShouldBe(new DateTime(2024, 12, 31, 23, 0, 0, DateTimeKind.Utc));
        result[1].DateTime.ShouldBe(new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        result[2].DateTime.ShouldBe(new DateTime(2025, 1, 1, 1, 0, 0, DateTimeKind.Utc));
        result[3].DateTime.ShouldBe(new DateTime(2025, 1, 1, 2, 0, 0, DateTimeKind.Utc));
    }
    
    private static PowerTrade GetFullDayTrades(DateTime requestedDate)
    {
        // 24 periods for full day
        return PowerTrade.Create(requestedDate, 24);
    }

    private static void ValidateResultDateTimes(DateTime requestedDate, List<InputItemDto> result)
    {
        for (int i = 0; i < result.Count; i++)
        {
            var expectedTime = requestedDate.AddHours(i + 1);
            result[i].DateTime.ShouldBe(expectedTime);
            result[i].DateTime.Kind.ShouldBe(DateTimeKind.Utc);
        }
    }
}
