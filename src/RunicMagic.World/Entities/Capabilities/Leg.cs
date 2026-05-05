namespace RunicMagic.World.Entities.Capabilities;

public class Leg
{
    public Leg(long lateralOffset, long forwardOffset)
    {
        LateralOffset = lateralOffset;
        ForwardOffset = forwardOffset;
    }

    public long LateralOffset { get; }
    public long ForwardOffset { get; }

    public (double Rx, double Ry) GetWorldOffset(double entityAngle)
    {
        var cosA = Math.Cos(entityAngle);
        var sinA = Math.Sin(entityAngle);
        var rx = LateralOffset * sinA + ForwardOffset * cosA;
        var ry = -LateralOffset * cosA + ForwardOffset * sinA;
        return (rx, ry);
    }
}
