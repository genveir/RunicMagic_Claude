using FluentAssertions;
using RunicMagic.World.Geometry;
using Xunit;

namespace RunicMagic.Tests.Geometry;

public class RectangleIntersectionTests
{
    // Rectangle centred at (500, 0), 100x100 → spans x:[450,550], y:[-50,50]
    private static readonly Rectangle Target = new(new Location(500, 0), Width: 100, Height: 100, Angle: 0.0d);

    [Fact]
    public void IntersectsRay_RayHitsFront_ReturnsTrueAndPositiveT()
    {
        var hit = Target.IntersectsRay(new Location(0, 0), new Direction(1, 0), out var t);

        hit.Should().BeTrue();
        t.Should().BeApproximately(450, precision: 0.001);
    }

    [Fact]
    public void IntersectsRay_RayMisses_ReturnsFalse()
    {
        var hit = Target.IntersectsRay(new Location(0, 0), new Direction(0, 1), out _);

        hit.Should().BeFalse();
    }

    [Fact]
    public void IntersectsRay_RayPointedAway_ReturnsFalse()
    {
        var hit = Target.IntersectsRay(new Location(0, 0), new Direction(-1, 0), out _);

        hit.Should().BeFalse();
    }

    [Fact]
    public void IntersectsRay_RayParallelInsideSlab_HitsOnY()
    {
        // Ray at y=0 heading right hits the rectangle
        var hit = Target.IntersectsRay(new Location(0, 0), new Direction(1, 0), out var t);

        hit.Should().BeTrue();
        t.Should().BePositive();
    }

    [Fact]
    public void IntersectsRay_RayParallelOutsideSlab_ReturnsFalse()
    {
        // Ray at y=200 heading right — outside y slab of target
        var hit = Target.IntersectsRay(new Location(0, 200), new Direction(1, 0), out _);

        hit.Should().BeFalse();
    }

    [Fact]
    public void IntersectsRay_OriginInsideRectangle_ReturnsFalse()
    {
        // Origin at centre of the rectangle — entry t is negative
        var hit = Target.IntersectsRay(new Location(500, 0), new Direction(1, 0), out _);

        hit.Should().BeFalse();
    }

    [Fact]
    public void IntersectsRay_DiagonalRayHitsCorner_ReturnsTrue()
    {
        var hit = Target.IntersectsRay(new Location(0, -500), new Direction(1, 1), out var t);

        hit.Should().BeTrue();
        t.Should().BePositive();
    }
}

public class RectangleRotateTests
{
    [Fact]
    public void Rotate_ZeroAngle_ReturnsUnchangedRectangle()
    {
        var rect = new Rectangle(new Location(100, 0), Width: 40, Height: 20, Angle: 0);

        var result = rect.Rotate(0);

        result.Should().Be(rect);
    }

    [Fact]
    public void Rotate_QuarterTurn_MovesLocationCorrectly()
    {
        // Centre at (100, 0), rotated 90°: new centre should be at (0, 100)
        var rect = new Rectangle(new Location(100, 0), Width: 40, Height: 20, Angle: 0);

        var result = rect.Rotate(Math.PI / 2);

        result.Location.X.Should().BeApproximately(0, precision: 1e-10);
        result.Location.Y.Should().BeApproximately(100, precision: 1e-10);
    }

    [Fact]
    public void Rotate_IncrementsAngle()
    {
        var rect = new Rectangle(new Location(0, 0), Width: 40, Height: 20, Angle: Math.PI / 4);

        var result = rect.Rotate(Math.PI / 4);

        result.Angle.Should().BeApproximately(Math.PI / 2, precision: 1e-10);
    }

    [Fact]
    public void Rotate_PreservesWidthAndHeight()
    {
        var rect = new Rectangle(new Location(50, 30), Width: 40, Height: 20, Angle: 0);

        var result = rect.Rotate(1.23);

        result.Width.Should().Be(40);
        result.Height.Should().Be(20);
    }
}

public class RectangleIntersectsWithTests
{
    [Fact]
    public void AxisAligned_Overlapping_ReturnsTrue()
    {
        var a = new Rectangle(new Location(0, 0), Width: 100, Height: 100, Angle: 0);
        var b = new Rectangle(new Location(50, 0), Width: 100, Height: 100, Angle: 0);

        a.IntersectsWith(b).Should().BeTrue();
    }

    [Fact]
    public void AxisAligned_NonOverlapping_ReturnsFalse()
    {
        var a = new Rectangle(new Location(0, 0), Width: 100, Height: 100, Angle: 0);
        var b = new Rectangle(new Location(200, 0), Width: 100, Height: 100, Angle: 0);

        a.IntersectsWith(b).Should().BeFalse();
    }

    [Fact]
    public void OneFullyInsideOther_ReturnsTrue()
    {
        var outer = new Rectangle(new Location(0, 0), Width: 200, Height: 200, Angle: 0);
        var inner = new Rectangle(new Location(0, 0), Width: 50, Height: 50, Angle: 0);

        outer.IntersectsWith(inner).Should().BeTrue();
    }

    [Fact]
    public void CrossingRects_ReturnsTrue()
    {
        // Two thin rects centred at origin, one horizontal, one vertical — they cross in the middle.
        var horizontal = new Rectangle(new Location(0, 0), Width: 200, Height: 10, Angle: 0);
        var vertical = new Rectangle(new Location(0, 0), Width: 200, Height: 10, Angle: Math.PI / 2);

        horizontal.IntersectsWith(vertical).Should().BeTrue();
    }

    [Fact]
    public void RotatedRects_PassingAlongside_ReturnsFalse()
    {
        // Horizontal thin rect at y=0; small rect rotated 45° centred at (0,20).
        // Closest point of the rotated rect is y≈12.9, which does not reach y=5 (top of horizontal).
        var horizontal = new Rectangle(new Location(0, 0), Width: 200, Height: 10, Angle: 0);
        var rotated = new Rectangle(new Location(0, 20), Width: 10, Height: 10, Angle: Math.PI / 4);

        horizontal.IntersectsWith(rotated).Should().BeFalse();
    }
}

public class RectangleContainsRectangleTests
{
    [Fact]
    public void SmallRectInsideLargeRect_ReturnsTrue()
    {
        var container = new Rectangle(new Location(0, 0), Width: 200, Height: 200, Angle: 0);
        var inner = new Rectangle(new Location(0, 0), Width: 50, Height: 50, Angle: 0);

        var result = container.Contains(inner);

        result.Should().BeTrue();
    }

    [Fact]
    public void PartialOverlap_ReturnsFalse()
    {
        var container = new Rectangle(new Location(0, 0), Width: 100, Height: 100, Angle: 0);
        var inner = new Rectangle(new Location(60, 0), Width: 100, Height: 100, Angle: 0);

        var result = container.Contains(inner);

        result.Should().BeFalse();
    }

    [Fact]
    public void RotatedContainer_ContainsRectAlongLongAxis_ReturnsTrue()
    {
        // Container 200x40 rotated 90°: long axis now vertical, spans y:[-100,100], x:[-20,20].
        // Inner 10x10 at (0,50) fits — it would NOT fit in the unrotated version (y=50 > half-height=20).
        var container = new Rectangle(new Location(0, 0), Width: 200, Height: 40, Angle: Math.PI / 2);
        var inner = new Rectangle(new Location(0, 50), Width: 10, Height: 10, Angle: 0);

        var result = container.Contains(inner);

        result.Should().BeTrue();
    }

    [Fact]
    public void RotatedContainer_RejectsRectBeyondShortAxis_ReturnsFalse()
    {
        // Same container — inner at (25, 0) is beyond the short half-extent of 20.
        var container = new Rectangle(new Location(0, 0), Width: 200, Height: 40, Angle: Math.PI / 2);
        var inner = new Rectangle(new Location(25, 0), Width: 10, Height: 10, Angle: 0);

        var result = container.Contains(inner);

        result.Should().BeFalse();
    }

    [Fact]
    public void EqualSizedRects_InnerRotated45Degrees_DoesNotFit_ReturnsFalse()
    {
        // A 100x100 rect rotated 45° has corners at distance 50√2 ≈ 70.7 from centre,
        // which exceeds the half-extent of 50 of an equal-sized axis-aligned container.
        var container = new Rectangle(new Location(0, 0), Width: 100, Height: 100, Angle: 0);
        var inner = new Rectangle(new Location(0, 0), Width: 100, Height: 100, Angle: Math.PI / 4);

        var result = container.Contains(inner);

        result.Should().BeFalse();
    }
}

public class RectangleContainsLocationTests
{
    // Rectangle centred at (0, 0), 200x40, rotated 90°: long axis is now vertical.
    // Spans approximately x:[-20,20], y:[-100,100] in world space.
    private static readonly Rectangle Rotated = new(new Location(0, 0), Width: 200, Height: 40, Angle: Math.PI / 2);

    [Fact]
    public void PointInsideRotatedRectangle_ReturnsTrue()
    {
        // (0, 30) is outside the unrotated footprint (y=30 > half-height=20) but inside the rotated one
        var result = Rotated.Contains(new Location(0, 30));

        result.Should().BeTrue();
    }

    [Fact]
    public void PointOutsideRotatedRectangle_ReturnsFalse()
    {
        // (30, 0) is inside the unrotated footprint (x=30 < half-width=100) but outside the rotated one
        var result = Rotated.Contains(new Location(30, 0));

        result.Should().BeFalse();
    }
}

public class RectangleIntersectsRayRotatedTests
{
    // 200x10 rotated 90°: long axis is now vertical, spans world x:[-5,5], y:[-100,100].
    private static readonly Rectangle Vertical = new(new Location(0, 0), Width: 200, Height: 10, Angle: Math.PI / 2);

    [Fact]
    public void RotatedRect_RayApproachingFromSide_Hits()
    {
        // Ray from (-50, 20) going right. y=20 is outside the unrotated rect's y:[-5,5],
        // but the rotated rect's short axis now runs along X so the ray hits it at x=-5.
        var hit = Vertical.IntersectsRay(new Location(-50, 20), new Direction(1, 0), out var t);

        hit.Should().BeTrue();
        t.Should().BeApproximately(45, precision: 0.001);
    }

    [Fact]
    public void RotatedRect_RayMissesPastShortAxis_ReturnsFalse()
    {
        // Ray from (-50, 20) going right, but aimed past the short axis (y=20 > half-height=5
        // in the rotated frame's X direction). Actually let's fire it at y=8 which is outside
        // the short axis half-extent of 5.
        var hit = Vertical.IntersectsRay(new Location(-50, 8), new Direction(0, 1), out _);

        hit.Should().BeFalse();
    }
}

public class RectangleIsWithinDistanceFromPointRotatedTests
{
    // 200x10 rotated 90°: spans world x:[-5,5], y:[-100,100].
    private static readonly Rectangle Vertical = new(new Location(0, 0), Width: 200, Height: 10, Angle: Math.PI / 2);

    [Fact]
    public void RotatedRect_PointBeyondShortAxis_DistanceIsCorrect()
    {
        // Point at (50, 0). Nearest edge of rotated rect is at x=5, so distance = 45.
        // The same point would be inside the unrotated 200x10 rect (within x:[-100,100]).
        Vertical.IsWithinDistanceFromPoint(new Location(50, 0), maxDistance: 45).Should().BeTrue();
        Vertical.IsWithinDistanceFromPoint(new Location(50, 0), maxDistance: 44).Should().BeFalse();
    }
}

public class RectangleIsWithinDistanceFromPointTests
{
    // Rectangle centred at (0, 0), 100x100 → spans x:[-50,50], y:[-50,50]
    private static readonly Rectangle Box = new(new Location(0, 0), Width: 100, Height: 100, Angle: 0.0d);

    [Fact]
    public void PointInsideRectangle_IsWithinAnyPositiveDistance()
    {
        var result = Box.IsWithinDistanceFromPoint(new Location(0, 0), maxDistance: 1);

        result.Should().BeTrue();
    }

    [Fact]
    public void PointOnEdge_IsWithinZeroDistance()
    {
        var result = Box.IsWithinDistanceFromPoint(new Location(50, 0), maxDistance: 0);

        result.Should().BeTrue();
    }

    [Fact]
    public void PointExactlyAtMaxDistance_IsWithin()
    {
        // Point at (550, 0), box right edge at x=50 → gap = 500
        var result = Box.IsWithinDistanceFromPoint(new Location(550, 0), maxDistance: 500);

        result.Should().BeTrue();
    }

    [Fact]
    public void PointOneUnitBeyondMaxDistance_IsNotWithin()
    {
        // Point at (551, 0), box right edge at x=50 → gap = 501
        var result = Box.IsWithinDistanceFromPoint(new Location(551, 0), maxDistance: 500);

        result.Should().BeFalse();
    }

    [Fact]
    public void PointDiagonallyBeyondCorner_DistanceIsEuclidean()
    {
        // Corner at (50, 50); point at (50 + 400, 50 + 300) → dx=400, dy=300 → distance=500
        var result = Box.IsWithinDistanceFromPoint(new Location(450, 350), maxDistance: 500);

        result.Should().BeTrue();
    }

    [Fact]
    public void PointDiagonallyBeyondCorner_JustOutside_IsNotWithin()
    {
        // Corner at (50, 50); point at (50 + 400, 50 + 300) → distance=500; add 1 to x → distance > 500
        var result = Box.IsWithinDistanceFromPoint(new Location(451, 350), maxDistance: 500);

        result.Should().BeFalse();
    }
}
