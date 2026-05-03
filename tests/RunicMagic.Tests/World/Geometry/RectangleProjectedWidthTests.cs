using RunicMagic.World.Geometry;

namespace RunicMagic.Tests.World.Geometry;

public class RectangleProjectedWidthTests
{
    [Fact]
    public void GetProjectedWidth_AxisAligned_MovingInX_ReturnsHeight()
    {
        // Moving along the local X axis — only the H dimension is exposed
        var rect = new Rectangle(new Location(0, 0), Width: 200, Height: 100, Angle: 0);

        var projected = rect.GetProjectedWidth(new Direction(1, 0));

        projected.Should().BeApproximately(100, 0.001);
    }

    [Fact]
    public void GetProjectedWidth_AxisAligned_MovingInY_ReturnsWidth()
    {
        // Moving along the local Y axis — only the W dimension is exposed
        var rect = new Rectangle(new Location(0, 0), Width: 200, Height: 100, Angle: 0);

        var projected = rect.GetProjectedWidth(new Direction(0, 1));

        projected.Should().BeApproximately(200, 0.001);
    }

    [Fact]
    public void GetProjectedWidth_AxisAligned_MovingAt45Degrees_ReturnsMidValue()
    {
        // At 45°: projected = W×(√2/2) + H×(√2/2) = (W+H)×(√2/2)
        var rect = new Rectangle(new Location(0, 0), Width: 200, Height: 100, Angle: 0);
        var expected = (200 + 100) * (Math.Sqrt(2) / 2);

        var projected = rect.GetProjectedWidth(new Direction(Math.Sqrt(2) / 2, Math.Sqrt(2) / 2));

        projected.Should().BeApproximately(expected, 0.001);
    }

    [Fact]
    public void GetProjectedWidth_RotatedRectangle_AccountsForOrientation()
    {
        // Rectangle W=200, H=100, rotated 90°, moving in +X
        // After rotation, the 200mm extent is vertical — moving in +X exposes 200mm
        var rect = new Rectangle(new Location(0, 0), Width: 200, Height: 100, Angle: Math.PI / 2);

        var projected = rect.GetProjectedWidth(new Direction(1, 0));

        projected.Should().BeApproximately(200, 0.001);
    }
}
