using Pwr.Domain.Models;
using Shouldly;

namespace Pwr.UnitTests.Domain;

public class InputItemDtoTests
{
    [Fact]
    public void InputItemDto_DefaultConstructor_ShouldInitializeCorrectly()
    {
        // Act
        var dto = new InputItemDto();

        // Assert
        dto.DateTime.ShouldBe(default(DateTime));
        dto.Volume.ShouldBe(0.0);
    }

    [Fact]
    public void InputItemDto_WithValues_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var dateTime = new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Utc);
        var volume = 123.45;

        // Act
        var dto = new InputItemDto
        {
            DateTime = dateTime,
            Volume = volume
        };

        // Assert
        dto.DateTime.ShouldBe(dateTime);
        dto.Volume.ShouldBe(volume);
    }

    [Fact]
    public void InputItemDto_WithUtcDateTime_ShouldPreserveKind()
    {
        // Arrange
        var utcDateTime = new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Utc);

        // Act
        var dto = new InputItemDto
        {
            DateTime = utcDateTime,
            Volume = 100.0
        };

        // Assert
        dto.DateTime.Kind.ShouldBe(DateTimeKind.Utc);
        dto.DateTime.ShouldBe(utcDateTime);
    }

    [Fact]
    public void InputItemDto_WithLocalDateTime_ShouldPreserveKind()
    {
        // Arrange
        var localDateTime = new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Local);

        // Act
        var dto = new InputItemDto
        {
            DateTime = localDateTime,
            Volume = 100.0
        };

        // Assert
        dto.DateTime.Kind.ShouldBe(DateTimeKind.Local);
        dto.DateTime.ShouldBe(localDateTime);
    }

    [Fact]
    public void InputItemDto_WithUnspecifiedDateTime_ShouldPreserveKind()
    {
        // Arrange
        var unspecifiedDateTime = new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Unspecified);

        // Act
        var dto = new InputItemDto
        {
            DateTime = unspecifiedDateTime,
            Volume = 100.0
        };

        // Assert
        dto.DateTime.Kind.ShouldBe(DateTimeKind.Unspecified);
        dto.DateTime.ShouldBe(unspecifiedDateTime);
    }

    [Fact]
    public void InputItemDto_WithZeroVolume_ShouldAcceptZero()
    {
        // Act
        var dto = new InputItemDto
        {
            DateTime = DateTime.UtcNow,
            Volume = 0.0
        };

        // Assert
        dto.Volume.ShouldBe(0.0);
    }

    [Fact]
    public void InputItemDto_WithNegativeVolume_ShouldAcceptNegative()
    {
        // Act
        var dto = new InputItemDto
        {
            DateTime = DateTime.UtcNow,
            Volume = -123.45
        };

        // Assert
        dto.Volume.ShouldBe(-123.45);
    }

    [Fact]
    public void InputItemDto_WithMaxDoubleVolume_ShouldAcceptMaxValue()
    {
        // Act
        var dto = new InputItemDto
        {
            DateTime = DateTime.UtcNow,
            Volume = double.MaxValue
        };

        // Assert
        dto.Volume.ShouldBe(double.MaxValue);
    }

    [Fact]
    public void InputItemDto_WithMinDoubleVolume_ShouldAcceptMinValue()
    {
        // Act
        var dto = new InputItemDto
        {
            DateTime = DateTime.UtcNow,
            Volume = double.MinValue
        };

        // Assert
        dto.Volume.ShouldBe(double.MinValue);
    }

    [Fact]
    public void InputItemDto_WithNaNVolume_ShouldAcceptNaN()
    {
        // Act
        var dto = new InputItemDto
        {
            DateTime = DateTime.UtcNow,
            Volume = double.NaN
        };

        // Assert
        dto.Volume.ShouldBe(double.NaN);
    }

    [Fact]
    public void InputItemDto_WithInfinityVolume_ShouldAcceptInfinity()
    {
        // Act
        var dto = new InputItemDto
        {
            DateTime = DateTime.UtcNow,
            Volume = double.PositiveInfinity
        };

        // Assert
        dto.Volume.ShouldBe(double.PositiveInfinity);
    }
}
