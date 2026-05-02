using RunicMagic.Controller.Services;
using RunicMagic.Tests.Builders;
using RunicMagic.World.Geometry;
using RunicMagic.World.Services;

namespace RunicMagic.Tests.Locomotion;

public class LocomotionServiceTests
{
    [Fact]
    public void TryMove_ReturnsFalse_WhenEntityHasNoLocomotionCapability()
    {
        var entity = new EntityBuilder().Build();

        var result = LocomotionService.TryMove(entity, new Direction(1, 0), new EventTracker());

        result.Should().BeFalse();
    }

    [Fact]
    public void TryMove_ReturnsFalse_WhenEntityIsUnderEngineMotion()
    {
        var entity = new EntityBuilder().WithLocomotion().Build();
        entity.IsUnderEngineMotion = true;

        var result = LocomotionService.TryMove(entity, new Direction(1, 0), new EventTracker());

        result.Should().BeFalse();
    }

    [Fact]
    public void TryMove_ReturnsTrue_WhenMoveSucceeds()
    {
        var entity = new EntityBuilder().WithLocomotion().Build();

        var result = LocomotionService.TryMove(entity, new Direction(1, 0), new EventTracker());

        result.Should().BeTrue();
    }

    [Fact]
    public void TryMove_MovesEntityInGivenDirection()
    {
        var entity = new EntityBuilder().WithLocation(x: 0, y: 0).WithLocomotion().Build();

        LocomotionService.TryMove(entity, new Direction(0, 1), new EventTracker());

        entity.Location.Y.Should().BeApproximately(10, 0.001);
        entity.Location.X.Should().BeApproximately(0, 0.001);
    }

    [Fact]
    public void TryMove_DoesNotMoveEntity_WhenIsUnderEngineMotion()
    {
        var entity = new EntityBuilder().WithLocation(x: 0, y: 0).WithLocomotion().Build();
        entity.IsUnderEngineMotion = true;

        LocomotionService.TryMove(entity, new Direction(1, 0), new EventTracker());

        entity.Location.X.Should().BeApproximately(0, 0.001);
        entity.Location.Y.Should().BeApproximately(0, 0.001);
    }

    [Fact]
    public void TryMove_AccumulatesSpeed_AcrossConsecutiveTicks()
    {
        var entity = new EntityBuilder().WithLocation(x: 0, y: 0).WithLocomotion().Build();
        var direction = new Direction(1, 0);

        LocomotionService.TryMove(entity, direction, new EventTracker());
        var xAfterTick1 = entity.Location.X;

        LocomotionService.TryMove(entity, direction, new EventTracker());
        var xAfterTick2 = entity.Location.X;

        xAfterTick1.Should().BeApproximately(10, 0.001);
        xAfterTick2.Should().BeApproximately(30, 0.001);
    }

    [Fact]
    public void TryMove_TracksEntity_OnSuccess()
    {
        var entity = new EntityBuilder().WithLocomotion().Build();
        var tracker = new EventTracker();

        LocomotionService.TryMove(entity, new Direction(1, 0), tracker);

        tracker.TouchedEntities.Should().Contain(entity);
    }

    [Fact]
    public void TryMove_DoesNotAccumulateSpeed_WhenUnderEngineMotion()
    {
        var entity = new EntityBuilder().WithLocomotion().Build();
        entity.IsUnderEngineMotion = true;

        LocomotionService.TryMove(entity, new Direction(1, 0), new EventTracker());

        entity.Locomotion!.CurrentSpeed.Should().Be(0);
    }
}
