using RunicMagic.Controller.Models;
using RunicMagic.Controller.Services;
using RunicMagic.View.Models;
using RunicMagic.World;
using RunicMagic.World.Entities;
using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;

namespace RunicMagic.Tests.Controller.Services;

public class PlayerServiceTests
{
    private static (PlayerService service, WorldModel world) MakeService()
    {
        var world = new WorldModel();
        var spellCasting = new SpellCastingService(world, new SpellExecutor(world));
        var service = new PlayerService(world, spellCasting, new RayCastService(world));
        return (service, world);
    }

    private static Entity MakeAgencyEntity(long x, long y, string label = "agent")
    {
        return new EntityBuilder()
            .WithLabel(label)
            .WithLocation(x, y)
            .WithAgency()
            .WithLife(max: 10, current: 10)
            .Build();
    }

    [Fact]
    public async Task SetCaster_NoEntityAtPoint_ReturnsNoEntityEvent()
    {
        var (service, _) = MakeService();
        var eventTracker = new EventTracker();

        await service.SetCaster(new WorldCoordinate(1000, 1000));
        service.DrainAndFlush(eventTracker);

        eventTracker.ControllerEvents.Should().ContainSingle().Which.Should().BeOfType<NoEntitiesWithAgencyFoundEvent>();
    }

    [Fact]
    public async Task SetCaster_SingleAgencyEntityAtPoint_ReturnsCasterSetEvent()
    {
        var (service, world) = MakeService();
        var eventTracker = new EventTracker();

        var entity = MakeAgencyEntity(x: 0, y: 0, label: "hero");
        world.Add(entity);

        await service.SetCaster(new WorldCoordinate(0, 0));
        service.DrainAndFlush(eventTracker);

        eventTracker.ControllerEvents.Should().ContainSingle().Which.Should().BeOfType<CasterSetEvent>();
    }

    [Fact]
    public async Task SetCaster_SingleAgencyEntityAtPoint_ReturnsCasterId()
    {
        var (service, world) = MakeService();
        var eventTracker = new EventTracker();

        var entity = MakeAgencyEntity(x: 0, y: 0, label: "hero");
        world.Add(entity);

        await service.SetCaster(new WorldCoordinate(0, 0));
        service.DrainAndFlush(eventTracker);

        var result = service.GetCasterId();

        result.Should().Be(entity.Id);
    }

    [Fact]
    public async Task SetCaster_MultipleAgencyEntitiesAtPoint_ReturnsAmbiguousEvent()
    {
        var (service, world) = MakeService();
        var eventTracker = new EventTracker();

        world.Add(MakeAgencyEntity(x: 0, y: 0, label: "hero"));
        world.Add(MakeAgencyEntity(x: 0, y: 0, label: "villain"));

        await service.SetCaster(new WorldCoordinate(0, 0));
        service.DrainAndFlush(eventTracker);

        eventTracker.ControllerEvents.Should().ContainSingle().Which.Should().BeOfType<MultipleEntitiesWithAgencyFoundEvent>();
    }

    [Fact]
    public async Task MoveCaster_NoCasterSelected_EmitsNoCasterSelectedEvent()
    {
        var (service, _) = MakeService();
        var eventTracker = new EventTracker();

        await service.MoveCaster(new WorldCoordinate(100, 100));
        service.DrainAndFlush(eventTracker);

        eventTracker.ControllerEvents.Should().ContainSingle().Which.Should().BeOfType<NoCasterSelectedEvent>();
    }

    [Fact]
    public async Task MoveCaster_WithCasterSelected_UpdatesEntityPosition()
    {
        var (service, world) = MakeService();
        var entity = MakeAgencyEntity(x: 0, y: 0);
        world.Add(entity);
        await service.SetCaster(new WorldCoordinate(0, 0));
        await service.MoveCaster(new WorldCoordinate(500, 300));
        service.DrainAndFlush(new EventTracker());

        entity.Location.X.Should().Be(500);
        entity.Location.Y.Should().Be(300);
    }

    [Fact]
    public async Task MoveCaster_WithCasterSelected_EmitsCasterMovedEvent()
    {
        var (service, world) = MakeService();
        var entity = MakeAgencyEntity(x: 0, y: 0);
        world.Add(entity);
        await service.SetCaster(new WorldCoordinate(0, 0));
        service.DrainAndFlush(new EventTracker());

        var eventTracker = new EventTracker();
        await service.MoveCaster(new WorldCoordinate(500, 300));
        service.DrainAndFlush(eventTracker);

        eventTracker.ControllerEvents.Should().ContainSingle().Which.Should().BeOfType<CasterMovedEvent>();
    }

    [Fact]
    public async Task RegisterInput_NoCasterSelected_EmitsNoCasterSelectedEvent()
    {
        var (service, _) = MakeService();
        var eventTracker = new EventTracker();

        await service.RegisterInput("ZU VUN LA IR HOT IR HOT HOT");
        service.DrainAndFlush(eventTracker);

        eventTracker.ControllerEvents.Should().ContainSingle().Which.Should().BeOfType<NoCasterSelectedEvent>();
    }

    [Fact]
    public async Task SetPointingDirection_NoCasterSelected_EmitsNoCasterSelectedEvent()
    {
        var (service, _) = MakeService();
        var eventTracker = new EventTracker();

        await service.SetPointingDirection(new WorldCoordinate(500, 0));
        service.DrainAndFlush(eventTracker);

        eventTracker.ControllerEvents.Should().ContainSingle().Which.Should().BeOfType<NoCasterSelectedEvent>();
    }

    [Fact]
    public async Task SetPointingDirection_WithCasterSelected_SetsPointingDirection()
    {
        var (service, world) = MakeService();
        var entity = MakeAgencyEntity(x: 0, y: 0);
        world.Add(entity);
        await service.SetCaster(new WorldCoordinate(0, 0));
        await service.SetPointingDirection(new WorldCoordinate(1000, 0));
        service.DrainAndFlush(new EventTracker());

        entity.PointingDirection.Should().NotBeNull();
        entity.PointingDirection!.Value.X.Should().BeApproximately(1.0, precision: 0.001);
        entity.PointingDirection!.Value.Y.Should().BeApproximately(0.0, precision: 0.001);
    }

    [Fact]
    public async Task SetPointingDirection_WithCasterSelected_EmitsPointingDirectionSetEvent()
    {
        var (service, world) = MakeService();
        var entity = MakeAgencyEntity(x: 0, y: 0);
        world.Add(entity);
        await service.SetCaster(new WorldCoordinate(0, 0));
        service.DrainAndFlush(new EventTracker());

        var eventTracker = new EventTracker();
        await service.SetPointingDirection(new WorldCoordinate(1000, 0));
        service.DrainAndFlush(eventTracker);

        eventTracker.ControllerEvents.Should().ContainSingle().Which.Should().BeOfType<PointingDirectionSetEvent>();
    }

    [Fact]
    public async Task SetIndicateTarget_NoCasterSelected_EmitsNoCasterSelectedEvent()
    {
        var (service, _) = MakeService();
        var eventTracker = new EventTracker();

        await service.SetIndicateTarget(new WorldCoordinate(500, 0));
        service.DrainAndFlush(eventTracker);

        eventTracker.ControllerEvents.Should().ContainSingle().Which.Should().BeOfType<NoCasterSelectedEvent>();
    }

    [Fact]
    public async Task SetIndicateTarget_NothingAtPoint_EmitsNothingToIndicateEvent()
    {
        var (service, world) = MakeService();
        var caster = MakeAgencyEntity(x: 0, y: 0);
        world.Add(caster);
        await service.SetCaster(new WorldCoordinate(0, 0));
        service.DrainAndFlush(new EventTracker());

        var eventTracker = new EventTracker();
        await service.SetIndicateTarget(new WorldCoordinate(5000, 5000));
        service.DrainAndFlush(eventTracker);

        eventTracker.ControllerEvents.Should().ContainSingle().Which.Should().BeOfType<NothingToIndicateEvent>();
    }

    [Fact]
    public async Task SetIndicateTarget_TargetIsCaster_SetsIndicateTargetToSelfWithNullDirection()
    {
        var (service, world) = MakeService();
        var caster = MakeAgencyEntity(x: 0, y: 0);
        world.Add(caster);
        await service.SetCaster(new WorldCoordinate(0, 0));
        await service.SetIndicateTarget(new WorldCoordinate(0, 0));
        service.DrainAndFlush(new EventTracker());

        caster.IndicateTarget.Should().NotBeNull();
        caster.IndicateTarget!.EntityId.Should().Be(caster.Id);
        caster.IndicateTarget!.Direction.Should().BeNull();
    }

    [Fact]
    public async Task SetIndicateTarget_TargetIsCaster_EmitsIndicatingEventForCaster()
    {
        var (service, world) = MakeService();
        var caster = MakeAgencyEntity(x: 0, y: 0);
        world.Add(caster);
        await service.SetCaster(new WorldCoordinate(0, 0));
        service.DrainAndFlush(new EventTracker());

        var eventTracker = new EventTracker();
        await service.SetIndicateTarget(new WorldCoordinate(0, 0));
        service.DrainAndFlush(eventTracker);

        eventTracker.ControllerEvents.Should().ContainSingle()
            .Which.Should().BeOfType<IndicatingEvent>()
            .Which.Entity.Should().BeSameAs(caster);
    }

    [Fact]
    public async Task SetIndicateTarget_ObstacleBlocksTarget_EmitsIndicateTargetBlockedEvent()
    {
        var (service, world) = MakeService();
        var caster = MakeAgencyEntity(x: 0, y: 0);
        var obstacle = new EntityBuilder().WithLocation(200, 0).Build();
        var target = new EntityBuilder().WithLocation(500, 0).WithLabel("target").Build();
        world.Add(caster);
        world.Add(obstacle);
        world.Add(target);
        await service.SetCaster(new WorldCoordinate(0, 0));
        service.DrainAndFlush(new EventTracker());

        var eventTracker = new EventTracker();
        await service.SetIndicateTarget(new WorldCoordinate(500, 0));
        service.DrainAndFlush(eventTracker);

        eventTracker.ControllerEvents.Should().ContainSingle().Which.Should().BeOfType<IndicateTargetBlockedEvent>();
    }

    [Fact]
    public async Task SetIndicateTarget_TargetOutOfRange_EmitsIndicateTargetOutOfReachEvent()
    {
        var (service, world) = MakeService();
        var caster = MakeAgencyEntity(x: 0, y: 0);
        var target = new EntityBuilder().WithLocation(2000, 0).WithLabel("faraway").Build();
        world.Add(caster);
        world.Add(target);
        await service.SetCaster(new WorldCoordinate(0, 0));
        service.DrainAndFlush(new EventTracker());

        var eventTracker = new EventTracker();
        await service.SetIndicateTarget(new WorldCoordinate(2000, 0));
        service.DrainAndFlush(eventTracker);

        eventTracker.ControllerEvents.Should().ContainSingle().Which.Should().BeOfType<IndicateTargetOutOfReachEvent>();
    }

    [Fact]
    public async Task SetIndicateTarget_TargetInRange_SetsIndicateTargetWithDirection()
    {
        var (service, world) = MakeService();
        var caster = MakeAgencyEntity(x: 0, y: 0);
        var target = new EntityBuilder().WithLocation(500, 0).WithLabel("target").Build();
        world.Add(caster);
        world.Add(target);
        await service.SetCaster(new WorldCoordinate(0, 0));
        await service.SetIndicateTarget(new WorldCoordinate(500, 0));
        service.DrainAndFlush(new EventTracker());

        caster.IndicateTarget.Should().NotBeNull();
        caster.IndicateTarget!.EntityId.Should().Be(target.Id);
        caster.IndicateTarget!.Direction.Should().NotBeNull();
    }

    [Fact]
    public async Task SetIndicateTarget_TargetInRange_EmitsIndicatingEventForTarget()
    {
        var (service, world) = MakeService();
        var caster = MakeAgencyEntity(x: 0, y: 0);
        var target = new EntityBuilder().WithLocation(500, 0).WithLabel("target").Build();
        world.Add(caster);
        world.Add(target);
        await service.SetCaster(new WorldCoordinate(0, 0));
        service.DrainAndFlush(new EventTracker());

        var eventTracker = new EventTracker();
        await service.SetIndicateTarget(new WorldCoordinate(500, 0));
        service.DrainAndFlush(eventTracker);

        eventTracker.ControllerEvents.Should().ContainSingle()
            .Which.Should().BeOfType<IndicatingEvent>()
            .Which.Entity.Should().BeSameAs(target);
    }

    [Fact]
    public async Task DrainAndFlush_MultipleQueuedActions_AddsMultipleEvents()
    {
        var (service, world) = MakeService();
        var entity = MakeAgencyEntity(x: 0, y: 0);
        world.Add(entity);
        await service.SetCaster(new WorldCoordinate(0, 0));
        await service.MoveCaster(new WorldCoordinate(500, 300));

        var eventTracker = new EventTracker();
        service.DrainAndFlush(eventTracker);

        eventTracker.ControllerEvents.Should().HaveCount(2);
    }
}
