using FluentAssertions;
using RunicMagic.World;
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
