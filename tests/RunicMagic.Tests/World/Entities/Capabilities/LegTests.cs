using RunicMagic.World.Entities.Capabilities;

namespace RunicMagic.Tests.World.Entities.Capabilities;

public class LegTests
{
    // ── Facing east (angle = 0) ───────────────────────────────────────────────

    [Fact]
    public void GetWorldOffset_RightLeg_FacingEast_IsDirectlySouth()
    {
        var leg = new Leg(lateralOffset: 100, forwardOffset: 0);

        var (rx, ry) = leg.GetWorldOffset(entityAngle: 0.0);

        rx.Should().BeApproximately(0.0, precision: 1e-9);
        ry.Should().BeApproximately(-100.0, precision: 1e-9);
    }

    [Fact]
    public void GetWorldOffset_LeftLeg_FacingEast_IsDirectlyNorth()
    {
        var leg = new Leg(lateralOffset: -100, forwardOffset: 0);

        var (rx, ry) = leg.GetWorldOffset(entityAngle: 0.0);

        rx.Should().BeApproximately(0.0, precision: 1e-9);
        ry.Should().BeApproximately(100.0, precision: 1e-9);
    }

    // ── Facing north (angle = π/2) ────────────────────────────────────────────

    [Fact]
    public void GetWorldOffset_RightLeg_FacingNorth_IsDirectlyEast()
    {
        var leg = new Leg(lateralOffset: 100, forwardOffset: 0);

        var (rx, ry) = leg.GetWorldOffset(entityAngle: Math.PI / 2);

        rx.Should().BeApproximately(100.0, precision: 1e-9);
        ry.Should().BeApproximately(0.0, precision: 1e-9);
    }

    // ── Facing south (angle = 3π/2, like the guard) ───────────────────────────

    [Fact]
    public void GetWorldOffset_RightLeg_FacingSouth_IsDirectlyWest()
    {
        var leg = new Leg(lateralOffset: 100, forwardOffset: 0);

        var (rx, ry) = leg.GetWorldOffset(entityAngle: 3 * Math.PI / 2);

        rx.Should().BeApproximately(-100.0, precision: 1e-9);
        ry.Should().BeApproximately(0.0, precision: 1e-9);
    }

    // ── Forward offset ────────────────────────────────────────────────────────

    [Fact]
    public void GetWorldOffset_ForwardOffset_FacingEast_IsDirectlyEast()
    {
        var leg = new Leg(lateralOffset: 0, forwardOffset: 50);

        var (rx, ry) = leg.GetWorldOffset(entityAngle: 0.0);

        rx.Should().BeApproximately(50.0, precision: 1e-9);
        ry.Should().BeApproximately(0.0, precision: 1e-9);
    }
}
