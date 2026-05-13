using RunicMagic.World.Entities;
using RunicMagic.World.Entities.Capabilities;
using RunicMagic.World.Motion.Simulated;

namespace RunicMagic.Tests.World.Entities.Capabilities;

public class LocomotionCapabilityTests
{
    private static readonly Leg LeftLeg = new Leg(name: "Left", lateralOffset: -100, forwardOffset: 0);
    private static readonly Leg RightLeg = new Leg(name: "Right", lateralOffset: 100, forwardOffset: 0);

    private static LocomotionCapability MakeLocomotion(double efficiency = 1.0)
    {
        var legs = new List<Leg> { LeftLeg, RightLeg };
        return new LocomotionCapability(legs, efficiency);
    }

    private static Entity MakeEntity(
        long x = 0, long y = 0,
        long strength = 1000,
        long weight = 1000,
        long width = 200, long height = 200,
        double angle = 0,
        double dragCoefficient = 0,
        double angularDragCoefficient = 0)
    {
        return new EntityBuilder()
            .WithLocation(x, y)
            .WithStrength(strength)
            .WithWeight(weight)
            .WithSize(width, height)
            .WithAngle(angle)
            .WithDragCoefficient(dragCoefficient)
            .WithAngularDragCoefficient(angularDragCoefficient)
            .Build();
    }

    // --- LegNames ---

    [Fact]
    public void LegNames_ReturnsNamesOfAllLegs()
    {
        var locomotion = MakeLocomotion();

        var names = locomotion.LegNames;

        names.Should().BeEquivalentTo(new[] { "Left", "Right" });
    }

    // --- ApplyLinearForce ---

    [Fact]
    public void ApplyLinearForce_PositiveFraction_AddsForwardImpulses()
    {
        var locomotion = MakeLocomotion();
        var entity = MakeEntity(angle: 0);

        locomotion.ApplyLinearForce(entity, forceFraction: 1.0);

        entity.PendingImpulses.Sum(v => v.Fx).Should().BeGreaterThan(0);
    }

    [Fact]
    public void ApplyLinearForce_NegativeFraction_AddsBackwardImpulses()
    {
        var locomotion = MakeLocomotion();
        var entity = MakeEntity(angle: 0);

        locomotion.ApplyLinearForce(entity, forceFraction: -1.0);

        entity.PendingImpulses.Sum(v => v.Fx).Should().BeLessThan(0);
    }

    [Fact]
    public void ApplyLinearForce_AddsOneImpulsePerLeg()
    {
        var locomotion = MakeLocomotion();
        var entity = MakeEntity();

        locomotion.ApplyLinearForce(entity, forceFraction: 1.0);

        entity.PendingImpulses.Should().HaveCount(2);
    }

    [Fact]
    public void ApplyLinearForce_ForceMagnitudeScalesByFraction()
    {
        var locomotion = MakeLocomotion();
        var fullEntity = MakeEntity();
        var halfEntity = MakeEntity();

        locomotion.ApplyLinearForce(fullEntity, forceFraction: 1.0);
        locomotion.ApplyLinearForce(halfEntity, forceFraction: 0.5);

        var fullFx = fullEntity.PendingImpulses.Sum(v => v.Fx);
        var halfFx = halfEntity.PendingImpulses.Sum(v => v.Fx);
        (halfFx / fullFx).Should().BeApproximately(0.5, 0.001);
    }

    // --- ApplyTurnForce ---

    [Fact]
    public void ApplyTurnForce_PositiveFraction_PositiveLateralLegGoesForward()
    {
        var locomotion = MakeLocomotion();
        var entity = MakeEntity(angle: 0);

        locomotion.ApplyTurnForce(entity, forceFraction: 1.0);

        // At angle=0: right leg (lateralOffset=+100) has Ry=-100, left leg (lateralOffset=-100) has Ry=+100.
        // Positive turn fraction: right leg (positive lateral) pushes forward, left leg pushes backward.
        var rightImpulse = entity.PendingImpulses.First(v => v.Ry < 0);
        var leftImpulse = entity.PendingImpulses.First(v => v.Ry > 0);
        rightImpulse.Fx.Should().BeGreaterThan(0);
        leftImpulse.Fx.Should().BeLessThan(0);
    }

    [Fact]
    public void ApplyTurnForce_NegativeFraction_NegativeLateralLegGoesForward()
    {
        var locomotion = MakeLocomotion();
        var entity = MakeEntity(angle: 0);

        locomotion.ApplyTurnForce(entity, forceFraction: -1.0);

        // At angle=0: right leg (lateralOffset=+100) has Ry=-100, left leg (lateralOffset=-100) has Ry=+100.
        // Negative turn fraction: left leg (negative lateral) pushes forward, right leg pushes backward.
        var rightImpulse = entity.PendingImpulses.First(v => v.Ry < 0);
        var leftImpulse = entity.PendingImpulses.First(v => v.Ry > 0);
        leftImpulse.Fx.Should().BeGreaterThan(0);
        rightImpulse.Fx.Should().BeLessThan(0);
    }

    [Fact]
    public void ApplyTurnForce_ZeroFraction_AddsNoImpulses()
    {
        var locomotion = MakeLocomotion();
        var entity = MakeEntity();

        locomotion.ApplyTurnForce(entity, forceFraction: 0.0);

        entity.PendingImpulses.Should().BeEmpty();
    }

    [Fact]
    public void ApplyTurnForce_MagnitudeScalesByAbsoluteFraction()
    {
        var locomotion = MakeLocomotion();
        var fullEntity = MakeEntity();
        var halfEntity = MakeEntity();

        locomotion.ApplyTurnForce(fullEntity, forceFraction: 1.0);
        locomotion.ApplyTurnForce(halfEntity, forceFraction: 0.5);

        var fullMag = fullEntity.PendingImpulses.Sum(v => Math.Abs(v.Fx));
        var halfMag = halfEntity.PendingImpulses.Sum(v => Math.Abs(v.Fx));
        (halfMag / fullMag).Should().BeApproximately(0.5, 0.001);
    }

    // --- ApplyLegForce ---

    [Fact]
    public void ApplyLegForce_NamedLeg_AddsOneImpulse()
    {
        var locomotion = MakeLocomotion();
        var entity = MakeEntity(angle: 0);

        locomotion.ApplyLegForce(entity, legName: "Right", forceFraction: 1.0);

        entity.PendingImpulses.Should().HaveCount(1);
    }

    [Fact]
    public void ApplyLegForce_PositiveFraction_AddsForwardImpulse()
    {
        var locomotion = MakeLocomotion();
        var entity = MakeEntity(angle: 0);

        locomotion.ApplyLegForce(entity, legName: "Left", forceFraction: 1.0);

        entity.PendingImpulses[0].Fx.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ApplyLegForce_UnknownName_AddsNoImpulse()
    {
        var locomotion = MakeLocomotion();
        var entity = MakeEntity();

        locomotion.ApplyLegForce(entity, legName: "BackLeft", forceFraction: 1.0);

        entity.PendingImpulses.Should().BeEmpty();
    }

    // --- ApplyLinearBrake ---

    [Fact]
    public void ApplyLinearBrake_NoVelocity_AddsNoImpulses()
    {
        var locomotion = MakeLocomotion();
        var entity = MakeEntity();

        locomotion.ApplyLinearBrake(entity, forceFraction: 1.0);

        entity.PendingImpulses.Should().BeEmpty();
    }

    [Fact]
    public void ApplyLinearBrake_WithForwardVelocity_AddsBackwardImpulses()
    {
        var locomotion = MakeLocomotion();
        var entity = MakeEntity(angle: 0);
        entity.Velocity = new VelocityVector(Vx: 100, Vy: 0, Omega: 0);

        locomotion.ApplyLinearBrake(entity, forceFraction: 1.0);

        entity.PendingImpulses.Should().NotBeEmpty();
        entity.PendingImpulses.Sum(v => v.Fx).Should().BeLessThan(0);
    }

    [Fact]
    public void ApplyLinearBrake_DoesNotOvershoottZero()
    {
        // Very strong brake on a slow entity: without overshoot prevention the velocity would flip sign
        var legs = new List<Leg>
        {
            new Leg(name: "Left", lateralOffset: -100, forwardOffset: 0),
            new Leg(name: "Right", lateralOffset: 100, forwardOffset: 0),
        };
        var locomotion = new LocomotionCapability(legs, locomotionEfficiency: 1.0);
        var entity = new EntityBuilder()
            .WithStrength(100000)
            .WithWeight(1000)
            .WithSize(200, 200)
            .Build();
        entity.Velocity = new VelocityVector(Vx: 1, Vy: 0, Omega: 0);

        locomotion.ApplyLinearBrake(entity, forceFraction: 1.0);

        // After applying the impulses, check the next velocity would not reverse direction.
        // We verify indirectly: total braking Fx must not exceed what would zero out the velocity.
        var totalFx = entity.PendingImpulses.Sum(v => v.Fx);
        var (nextVx, _) = PhysicsService.CalculateLinearVelocity(
            initialVelocity: new VelocityVector(Vx: 1, Vy: 0, Omega: 0),
            weight: 1000,
            dragCoefficient: 0,
            bounds: entity.Bounds,
            fx: totalFx,
            fy: 0,
            groundFrictionCoefficient: 0,
            isGrounded: false);

        (nextVx == 0.0 || Math.Sign(nextVx) == Math.Sign(1.0)).Should().BeTrue();
    }

    // --- ApplyAngularBrake ---

    [Fact]
    public void ApplyAngularBrake_NoOmega_AddsNoImpulses()
    {
        var locomotion = MakeLocomotion();
        var entity = MakeEntity();

        locomotion.ApplyAngularBrake(entity, forceFraction: 1.0);

        entity.PendingImpulses.Should().BeEmpty();
    }

    [Fact]
    public void ApplyAngularBrake_WithOmega_AddsImpulses()
    {
        var locomotion = MakeLocomotion();
        var entity = MakeEntity();
        entity.Velocity = new VelocityVector(Vx: 0, Vy: 0, Omega: 0.5);

        locomotion.ApplyAngularBrake(entity, forceFraction: 1.0);

        entity.PendingImpulses.Should().NotBeEmpty();
    }

    // --- GetLinearStopInfo ---

    [Fact]
    public void GetLinearStopInfo_ZeroVelocity_ReturnsZeroTicksAndDistance()
    {
        var locomotion = MakeLocomotion();
        var entity = MakeEntity();

        var (ticks, distance) = locomotion.GetLinearStopInfo(entity, forceFraction: 1.0);

        ticks.Should().Be(0L);
        distance.Should().Be(0.0);
    }

    [Fact]
    public void GetLinearStopInfo_WithVelocity_ReturnsPositiveTicksAndDistance()
    {
        var locomotion = MakeLocomotion();
        var entity = MakeEntity();
        entity.Velocity = new VelocityVector(Vx: 100, Vy: 0, Omega: 0);

        var (ticks, distance) = locomotion.GetLinearStopInfo(entity, forceFraction: 1.0);

        ticks.Should().BeGreaterThan(0L);
        distance.Should().BeGreaterThan(0.0);
    }

    [Fact]
    public void GetLinearStopInfo_LargerVelocity_RequiresMoreTicksAndDistance()
    {
        var locomotion = MakeLocomotion();
        var slowEntity = MakeEntity();
        slowEntity.Velocity = new VelocityVector(Vx: 50, Vy: 0, Omega: 0);
        var fastEntity = MakeEntity();
        fastEntity.Velocity = new VelocityVector(Vx: 200, Vy: 0, Omega: 0);

        var (slowTicks, slowDistance) = locomotion.GetLinearStopInfo(slowEntity, forceFraction: 1.0);
        var (fastTicks, fastDistance) = locomotion.GetLinearStopInfo(fastEntity, forceFraction: 1.0);

        fastTicks.Should().BeGreaterThanOrEqualTo(slowTicks);
        fastDistance.Should().BeGreaterThan(slowDistance);
    }

    // --- GetAngularStopInfo ---

    [Fact]
    public void GetAngularStopInfo_ZeroOmega_ReturnsZeroTicksAndAngle()
    {
        var locomotion = MakeLocomotion();
        var entity = MakeEntity();

        var (ticks, angle) = locomotion.GetAngularStopInfo(entity, forceFraction: 1.0);

        ticks.Should().Be(0L);
        angle.Should().Be(0.0);
    }

    [Fact]
    public void GetAngularStopInfo_WithOmega_ReturnsPositiveTicksAndAngle()
    {
        var locomotion = MakeLocomotion();
        var entity = MakeEntity();
        entity.Velocity = new VelocityVector(Vx: 0, Vy: 0, Omega: 0.1);

        var (ticks, angle) = locomotion.GetAngularStopInfo(entity, forceFraction: 1.0);

        ticks.Should().BeGreaterThan(0L);
        angle.Should().BeGreaterThan(0.0);
    }

    [Fact]
    public void GetAngularStopInfo_LargerOmega_RequiresMoreTicksAndAngle()
    {
        var locomotion = MakeLocomotion();
        var slowEntity = MakeEntity();
        slowEntity.Velocity = new VelocityVector(Vx: 0, Vy: 0, Omega: 0.05);
        var fastEntity = MakeEntity();
        fastEntity.Velocity = new VelocityVector(Vx: 0, Vy: 0, Omega: 0.2);

        var (slowTicks, slowAngle) = locomotion.GetAngularStopInfo(slowEntity, forceFraction: 1.0);
        var (fastTicks, fastAngle) = locomotion.GetAngularStopInfo(fastEntity, forceFraction: 1.0);

        fastTicks.Should().BeGreaterThanOrEqualTo(slowTicks);
        fastAngle.Should().BeGreaterThan(slowAngle);
    }

    [Fact]
    public void GetAngularStopInfo_LegsOnCenterline_ReturnsZero()
    {
        // Legs at lateralOffset=0 produce no torque, so braking is impossible
        var legs = new List<Leg> { new Leg(name: "Center", lateralOffset: 0, forwardOffset: 0) };
        var locomotion = new LocomotionCapability(legs, locomotionEfficiency: 1.0);
        var entity = MakeEntity();
        entity.Velocity = new VelocityVector(Vx: 0, Vy: 0, Omega: 0.1);

        var (ticks, angle) = locomotion.GetAngularStopInfo(entity, forceFraction: 1.0);

        ticks.Should().Be(0L);
        angle.Should().Be(0.0);
    }
}
