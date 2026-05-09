namespace RunicMagic.World.Motion.Simulated;

public readonly record struct VelocityVector(double Vx, double Vy, double Omega = 0.0)
{
    public static VelocityVector Zero => new(0.0, 0.0, 0.0);

    public double Speed => Math.Sqrt(Vx * Vx + Vy * Vy);
}
