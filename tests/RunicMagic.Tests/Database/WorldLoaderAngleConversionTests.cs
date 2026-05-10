using RunicMagic.Database;

namespace RunicMagic.Tests.Database;

public class WorldLoaderAngleConversionTests
{
    [Fact]
    public void CompassToDrawRadians_North_ReturnsHalfPi()
    {
        var result = WorldLoader.CompassToDrawRadians(0.0);

        result.Should().BeApproximately(Math.PI / 2.0, 1e-10);
    }

    [Fact]
    public void CompassToDrawRadians_East_ReturnsZero()
    {
        var result = WorldLoader.CompassToDrawRadians(Math.PI / 2.0);

        result.Should().BeApproximately(0.0, 1e-10);
    }

    [Fact]
    public void CompassToDrawRadians_South_ReturnsNegativeHalfPi()
    {
        var result = WorldLoader.CompassToDrawRadians(Math.PI);

        result.Should().BeApproximately(-Math.PI / 2.0, 1e-10);
    }

    [Fact]
    public void CompassToDrawRadians_West_ReturnsNegativePi()
    {
        var result = WorldLoader.CompassToDrawRadians(3.0 * Math.PI / 2.0);

        result.Should().BeApproximately(-Math.PI, 1e-10);
    }
}
