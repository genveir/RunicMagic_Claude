using FluentAssertions;
using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;
using Xunit;

namespace RunicMagic.Tests.Geometry;

public class DirectionTests
{
    [Fact]
    public void FromPoints_ReturnsNormalizedDirection()
    {
        var from = new Location(0, 0);
        var to = new Location(3, 4);

        var direction = Direction.FromPoints(from, to);

        direction.X.Should().BeApproximately(0.6, 1e-10);
        direction.Y.Should().BeApproximately(0.8, 1e-10);
    }

    [Fact]
    public void FromPoints_CoincidentPoints_ReturnsUnitLengthDirection()
    {
        var from = new Location(5, 5);
        var to = new Location(5, 5);

        var direction = Direction.FromPoints(from, to);

        var length = Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y);
        length.Should().BeApproximately(1.0, 1e-10);
    }

    [Fact]
    public void FromPoints_ResultHasUnitLength()
    {
        var from = new Location(0, 0);
        var to = new Location(7, 24);

        var direction = Direction.FromPoints(from, to);

        var length = Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y);
        length.Should().BeApproximately(1.0, 1e-10);
    }
}
