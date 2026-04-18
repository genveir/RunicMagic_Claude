using FluentAssertions;
using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;
using Xunit;

namespace RunicMagic.Tests.Geometry;

public class LocationExtensionsTests
{
    [Fact]
    public void Translate_MovesPointInGivenDirection()
    {
        var origin = new Location(0, 0);
        var direction = new Direction(1.0, 0.0);

        var result = origin.Translate(direction, 500);

        result.Should().Be(new Location(500, 0));
    }

    [Fact]
    public void Translate_RoundsToNearestMillimetre()
    {
        var origin = new Location(0, 0);
        // 45-degree diagonal: each component is 1/√2 ≈ 0.7071
        var diagonal = 1.0 / Math.Sqrt(2.0);
        var direction = new Direction(diagonal, diagonal);

        var result = origin.Translate(direction, 100);

        // 100 * (1/√2) ≈ 70.71 → rounds to 71
        result.Should().Be(new Location(71, 71));
    }

    [Fact]
    public void Translate_WorksFromNonZeroOrigin()
    {
        var origin = new Location(200, 300);
        var direction = new Direction(0.0, 1.0);

        var result = origin.Translate(direction, 150);

        result.Should().Be(new Location(200, 450));
    }

    [Fact]
    public void Centroid_ReturnsMidpointOfTwoLocations()
    {
        var locations = new[] { new Location(0, 0), new Location(100, 200) };

        var result = locations.Centroid();

        result.Should().Be(new Location(50, 100));
    }

    [Fact]
    public void Centroid_RoundsToNearestMillimetre()
    {
        var locations = new[] { new Location(0, 0), new Location(1, 0), new Location(2, 0) };

        // Average X = 1, Average Y = 0
        var result = locations.Centroid();

        result.Should().Be(new Location(1, 0));
    }

    [Fact]
    public void Centroid_SingleLocation_ReturnsThatLocation()
    {
        var locations = new[] { new Location(42, 99) };

        var result = locations.Centroid();

        result.Should().Be(new Location(42, 99));
    }

    [Fact]
    public void WeightedCentroid_PullsTowardHeavierPoint()
    {
        var items = new[]
        {
            (new Location(0, 0), 1L),
            (new Location(100, 0), 3L),
        };

        var result = items.WeightedCentroid();

        result.Should().Be(new Location(75, 0));
    }

    [Fact]
    public void WeightedCentroid_EqualWeights_MatchesUnweightedCentroid()
    {
        var items = new[]
        {
            (new Location(0, 0), 2L),
            (new Location(100, 200), 2L),
        };

        var result = items.WeightedCentroid();

        result.Should().Be(new Location(50, 100));
    }

    [Fact]
    public void WeightedCentroid_AllZeroWeights_FallsBackToUnweightedCentroid()
    {
        var items = new[]
        {
            (new Location(0, 0), 0L),
            (new Location(100, 0), 0L),
        };

        var result = items.WeightedCentroid();

        result.Should().Be(new Location(50, 0));
    }
}
