using Pwr.Domain.Models;
using Shouldly;

namespace Pwr.UnitTests.Domain;

public class OutputItemDtoTests
{
    [Fact]
    public void OutputItemDto_DefaultConstructor_ShouldInitializeCorrectly()
    {
        // Act
        var dto = new OutputItemDto();

        // Assert
        dto.DateTime.ShouldBe(string.Empty);
        dto.Volume.ShouldBe(0.0);
    }

    [Fact]
    public void OutputItemDto_WithValues_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var dateTime = "2024-06-01T12:00:00Z";
        var volume = 123.45;

        // Act
        var dto = new OutputItemDto
        {
            DateTime = dateTime,
            Volume = volume
        };

        // Assert
        dto.DateTime.ShouldBe(dateTime);
        dto.Volume.ShouldBe(volume);
    }

    [Fact]
    public void FromInputItemDto_WithUtcDateTime_ShouldConvertCorrectly()
    {
        // Arrange
        var inputDto = new InputItemDto
        {
            DateTime = new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Utc),
            Volume = 123.45
        };

        // Act
        var outputDto = OutputItemDto.FromInputItemDto(inputDto);

        // Assert
        outputDto.DateTime.ShouldBe("2024-06-01T12:00:00Z");
        outputDto.Volume.ShouldBe(123.45);
    }

    [Fact]
    public void FromInputItemDto_WithLocalDateTime_ShouldConvertToUtc()
    {
        // Arrange
        var localDateTime = new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Local);
        var inputDto = new InputItemDto
        {
            DateTime = localDateTime,
            Volume = 123.45
        };

        // Act
        var outputDto = OutputItemDto.FromInputItemDto(inputDto);

        // Assert
        outputDto.DateTime.ShouldContain("2024-06-01T");
        outputDto.DateTime.ShouldEndWith("Z");
        outputDto.Volume.ShouldBe(123.45);
    }

    [Fact]
    public void FromInputItemDto_WithUnspecifiedDateTime_ShouldConvertCorrectly()
    {
        // Arrange
        var inputDto = new InputItemDto
        {
            DateTime = new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Unspecified),
            Volume = 123.45
        };

        // Act
        var outputDto = OutputItemDto.FromInputItemDto(inputDto);

        // Assert
        outputDto.DateTime.ShouldContain("2024-06-01T");
        outputDto.DateTime.ShouldEndWith("Z");
        outputDto.Volume.ShouldBe(123.45);
    }

    [Fact]
    public void FromInputItemDto_WithZeroVolume_ShouldConvertCorrectly()
    {
        // Arrange
        var inputDto = new InputItemDto
        {
            DateTime = new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Utc),
            Volume = 0.0
        };

        // Act
        var outputDto = OutputItemDto.FromInputItemDto(inputDto);

        // Assert
        outputDto.DateTime.ShouldBe("2024-06-01T12:00:00Z");
        outputDto.Volume.ShouldBe(0.0);
    }

    [Fact]
    public void FromInputItemDto_WithNegativeVolume_ShouldConvertCorrectly()
    {
        // Arrange
        var inputDto = new InputItemDto
        {
            DateTime = new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Utc),
            Volume = -123.45
        };

        // Act
        var outputDto = OutputItemDto.FromInputItemDto(inputDto);

        // Assert
        outputDto.DateTime.ShouldBe("2024-06-01T12:00:00Z");
        outputDto.Volume.ShouldBe(-123.45);
    }

    [Fact]
    public void FromInputItemDto_WithMaxDoubleVolume_ShouldConvertCorrectly()
    {
        // Arrange
        var inputDto = new InputItemDto
        {
            DateTime = new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Utc),
            Volume = double.MaxValue
        };

        // Act
        var outputDto = OutputItemDto.FromInputItemDto(inputDto);

        // Assert
        outputDto.DateTime.ShouldBe("2024-06-01T12:00:00Z");
        outputDto.Volume.ShouldBe(double.MaxValue);
    }

    [Fact]
    public void FromInputItemDto_WithMinDoubleVolume_ShouldConvertCorrectly()
    {
        // Arrange
        var inputDto = new InputItemDto
        {
            DateTime = new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Utc),
            Volume = double.MinValue
        };

        // Act
        var outputDto = OutputItemDto.FromInputItemDto(inputDto);

        // Assert
        outputDto.DateTime.ShouldBe("2024-06-01T12:00:00Z");
        outputDto.Volume.ShouldBe(double.MinValue);
    }

    [Fact]
    public void FromInputItemDto_WithNaNVolume_ShouldConvertCorrectly()
    {
        // Arrange
        var inputDto = new InputItemDto
        {
            DateTime = new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Utc),
            Volume = double.NaN
        };

        // Act
        var outputDto = OutputItemDto.FromInputItemDto(inputDto);

        // Assert
        outputDto.DateTime.ShouldBe("2024-06-01T12:00:00Z");
        outputDto.Volume.ShouldBe(double.NaN);
    }

    [Fact]
    public void FromInputItemDto_WithInfinityVolume_ShouldConvertCorrectly()
    {
        // Arrange
        var inputDto = new InputItemDto
        {
            DateTime = new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Utc),
            Volume = double.PositiveInfinity
        };

        // Act
        var outputDto = OutputItemDto.FromInputItemDto(inputDto);

        // Assert
        outputDto.DateTime.ShouldBe("2024-06-01T12:00:00Z");
        outputDto.Volume.ShouldBe(double.PositiveInfinity);
    }

    [Fact]
    public void FromInputItemDto_WithDifferentTimeZones_ShouldConvertToUtc()
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
            var inputDto = new InputItemDto
            {
                DateTime = dateTime,
                Volume = 100.0
            };

            // Act
            var outputDto = OutputItemDto.FromInputItemDto(inputDto);

            // Assert
            outputDto.DateTime.ShouldEndWith("Z");
            outputDto.Volume.ShouldBe(100.0);
        }
    }
}
