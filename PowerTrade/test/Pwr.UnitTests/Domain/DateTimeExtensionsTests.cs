using Pwr.Domain;

namespace Pwr.UnitTests.Domain;

public class DateTimeExtensionsTests
{
    [Fact]
    public void ToUniversalIso8601_ConvertsToUniversalIso8601Format()
    {
        // Arrange
        var localDateTime = new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Local);
        var expected = "2024-06-01T10:00:00Z";

        // Act
        var result = localDateTime.ToUniversalIso8601string();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToUniversalIso8601_HandlesUtcDateTime()
    {
        // Arrange
        var utcDateTime = new DateTime(2024, 6, 1, 10, 0, 0, DateTimeKind.Utc);
        var expected = "2024-06-01T10:00:00Z";

        // Act
        var result = utcDateTime.ToUniversalIso8601string();

        // Assert
        Assert.Equal(expected, result);
    }
}
