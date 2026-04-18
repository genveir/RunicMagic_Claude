using FluentAssertions;
using RunicMagic.World.Geometry;
using Xunit;

namespace RunicMagic.Tests.Geometry;

public class RectangleIntersectionTests
{
    // Rectangle centred at (500, 0), 100x100 → spans x:[450,550], y:[-50,50]
    private static readonly Rectangle Target = new(X: 500, Y: 0, Width: 100, Height: 100);

    [Fact]
    public void IntersectsRay_RayHitsFront_ReturnsTrueAndPositiveT()
    {
        var hit = Target.IntersectsRay(ox: 0, oy: 0, dx: 1, dy: 0, out var t);

        hit.Should().BeTrue();
        t.Should().BeApproximately(450, precision: 0.001);
    }

    [Fact]
    public void IntersectsRay_RayMisses_ReturnsFalse()
    {
        var hit = Target.IntersectsRay(ox: 0, oy: 0, dx: 0, dy: 1, out _);

        hit.Should().BeFalse();
    }

    [Fact]
    public void IntersectsRay_RayPointedAway_ReturnsFalse()
    {
        var hit = Target.IntersectsRay(ox: 0, oy: 0, dx: -1, dy: 0, out _);

        hit.Should().BeFalse();
    }

    [Fact]
    public void IntersectsRay_RayParallelInsideSlab_HitsOnY()
    {
        // Ray at y=0 heading right hits the rectangle
        var hit = Target.IntersectsRay(ox: 0, oy: 0, dx: 1, dy: 0, out var t);

        hit.Should().BeTrue();
        t.Should().BePositive();
    }

    [Fact]
    public void IntersectsRay_RayParallelOutsideSlab_ReturnsFalse()
    {
        // Ray at y=200 heading right — outside y slab of target
        var hit = Target.IntersectsRay(ox: 0, oy: 200, dx: 1, dy: 0, out _);

        hit.Should().BeFalse();
    }

    [Fact]
    public void IntersectsRay_OriginInsideRectangle_ReturnsFalse()
    {
        // Origin at centre of the rectangle — entry t is negative
        var hit = Target.IntersectsRay(ox: 500, oy: 0, dx: 1, dy: 0, out _);

        hit.Should().BeFalse();
    }

    [Fact]
    public void IntersectsRay_DiagonalRayHitsCorner_ReturnsTrue()
    {
        var hit = Target.IntersectsRay(ox: 0, oy: -500, dx: 1, dy: 1, out var t);

        hit.Should().BeTrue();
        t.Should().BePositive();
    }
}

public class RectangleIsWithinDistanceFromPointTests
{
    // Rectangle centred at (0, 0), 100x100 → spans x:[-50,50], y:[-50,50]
    private static readonly Rectangle Box = new(X: 0, Y: 0, Width: 100, Height: 100);

    [Fact]
    public void PointInsideRectangle_IsWithinAnyPositiveDistance()
    {
        var result = Box.IsWithinDistanceFromPoint(px: 0, py: 0, maxDistance: 1);

        result.Should().BeTrue();
    }

    [Fact]
    public void PointOnEdge_IsWithinZeroDistance()
    {
        var result = Box.IsWithinDistanceFromPoint(px: 50, py: 0, maxDistance: 0);

        result.Should().BeTrue();
    }

    [Fact]
    public void PointExactlyAtMaxDistance_IsWithin()
    {
        // Point at (550, 0), box right edge at x=50 → gap = 500
        var result = Box.IsWithinDistanceFromPoint(px: 550, py: 0, maxDistance: 500);

        result.Should().BeTrue();
    }

    [Fact]
    public void PointOneUnitBeyondMaxDistance_IsNotWithin()
    {
        // Point at (551, 0), box right edge at x=50 → gap = 501
        var result = Box.IsWithinDistanceFromPoint(px: 551, py: 0, maxDistance: 500);

        result.Should().BeFalse();
    }

    [Fact]
    public void PointDiagonallyBeyondCorner_DistanceIsEuclidean()
    {
        // Corner at (50, 50); point at (50 + 400, 50 + 300) → dx=400, dy=300 → distance=500
        var result = Box.IsWithinDistanceFromPoint(px: 450, py: 350, maxDistance: 500);

        result.Should().BeTrue();
    }

    [Fact]
    public void PointDiagonallyBeyondCorner_JustOutside_IsNotWithin()
    {
        // Corner at (50, 50); point at (50 + 400, 50 + 300) → distance=500; add 1 to x → distance > 500
        var result = Box.IsWithinDistanceFromPoint(px: 451, py: 350, maxDistance: 500);

        result.Should().BeFalse();
    }
}
