using FluentAssertions;
using RunicMagic.Tests.Builders;
using RunicMagic.World;
using RunicMagic.World.Geometry;
using Xunit;

namespace RunicMagic.Tests.Geometry;

public class LocationExtensionsTests
{
    private static Entity MakeEntity(long x, long y, long width = 100, long height = 100)
    {
        return new EntityBuilder().WithLocation(x, y).WithSize(width, height).Build();
    }

    // ── WithinDistance ────────────────────────────────────────────────────────

    [Fact]
    public void WithinDistance_EntityWithinRadius_IsIncluded()
    {
        var entity = MakeEntity(x: 300, y: 0);

        var result = new[] { entity }.WithinDistance(new Location(0, 0), distance: 500);

        result.Should().ContainSingle().Which.Should().BeSameAs(entity);
    }

    [Fact]
    public void WithinDistance_EntityOutsideRadius_IsExcluded()
    {
        var entity = MakeEntity(x: 1000, y: 0);

        var result = new[] { entity }.WithinDistance(new Location(0, 0), distance: 500);

        result.Should().BeEmpty();
    }

    [Fact]
    public void WithinDistance_MeasuresNearestEdgeNotCenter()
    {
        // Entity centre at (600, 0), width 100 → nearest edge at x=550, 550mm from origin
        var entity = MakeEntity(x: 600, y: 0);

        var within = new[] { entity }.WithinDistance(new Location(0, 0), distance: 550);
        var outside = new[] { entity }.WithinDistance(new Location(0, 0), distance: 549);

        within.Should().ContainSingle();
        outside.Should().BeEmpty();
    }

    // ── Translate ─────────────────────────────────────────────────────────────

    [Fact]
    public void Translate_MovesPointInGivenDirection()
    {
        var origin = new Location(0, 0);
        var direction = new Direction(1.0, 0.0);

        var result = origin.Translate(direction, 500);

        result.Should().Be(new Location(500, 0));
    }

    [Fact]
    public void Translate_ReturnsExactFloatingPointPosition()
    {
        var origin = new Location(0, 0);
        var diagonal = 1.0 / Math.Sqrt(2.0);
        var direction = new Direction(diagonal, diagonal);

        var result = origin.Translate(direction, 100);

        // 100 * (1/√2) ≈ 70.711 — exact double result, no rounding
        result.X.Should().BeApproximately(70.711, precision: 0.001);
        result.Y.Should().BeApproximately(70.711, precision: 0.001);
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
