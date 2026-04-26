using FluentAssertions;
using RunicMagic.Controller.Models;
using RunicMagic.Controller.Services;
using RunicMagic.Tests.Builders;
using RunicMagic.World;
using RunicMagic.World.Capabilities;
using RunicMagic.World.Geometry;
using Xunit;

namespace RunicMagic.Tests;

public class PlayerServiceTests
{
    private static (PlayerService service, WorldModel world) MakeService()
    {
        var world = new WorldModel();
        var worldRendering = new WorldRenderingService(world, new RayCastService(world));
        var spellCasting = new SpellCastingService(world, new SpellExecutor(world));
        var teleport = new TeleportEntityService();
        var service = new PlayerService(world, worldRendering, spellCasting, teleport, new RayCastService(world));
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
    public async Task SetCaster_NoEntityAtPoint_ReturnsNoEntityMessage()
    {
        var (service, _) = MakeService();

        var result = await service.SetCaster(new WorldCoordinate(1000, 1000));

        result.Text.Should().ContainSingle().Which.Should().Contain("No entities with agency");
    }

    [Fact]
    public async Task SetCaster_SingleAgencyEntityAtPoint_ReturnsCasterSetMessage()
    {
        var (service, world) = MakeService();
        var entity = MakeAgencyEntity(x: 0, y: 0, label: "hero");
        world.Add(entity);

        var result = await service.SetCaster(new WorldCoordinate(0, 0));

        result.Text.Should().ContainSingle().Which.Should().Contain("hero");
    }

    [Fact]
    public async Task SetCaster_SingleAgencyEntityAtPoint_MarksEntityAsCasterInRenderingOutput()
    {
        var (service, world) = MakeService();
        var entity = MakeAgencyEntity(x: 0, y: 0, label: "hero");
        world.Add(entity);

        var result = await service.SetCaster(new WorldCoordinate(0, 0));

        result.Entities.Should().Contain(m => m.Label == "hero" && m.IsCaster);
    }

    [Fact]
    public async Task SetCaster_MultipleAgencyEntitiesAtPoint_ReturnsAmbiguousMessage()
    {
        var (service, world) = MakeService();
        world.Add(MakeAgencyEntity(x: 0, y: 0, label: "hero"));
        world.Add(MakeAgencyEntity(x: 0, y: 0, label: "villain"));

        var result = await service.SetCaster(new WorldCoordinate(0, 0));

        result.Text.Should().ContainSingle().Which.Should().Contain("Multiple entities");
    }

    [Fact]
    public async Task MoveCaster_NoCasterSelected_ReturnsNoCasterSelectedMessage()
    {
        var (service, _) = MakeService();

        var result = await service.MoveCaster(new WorldCoordinate(100, 100));

        result.Text.Should().ContainSingle().Which.Should().Contain("No caster selected");
    }

    [Fact]
    public async Task MoveCaster_WithCasterSelected_UpdatesEntityPosition()
    {
        var (service, world) = MakeService();
        var entity = MakeAgencyEntity(x: 0, y: 0);
        world.Add(entity);
        await service.SetCaster(new WorldCoordinate(0, 0));

        await service.MoveCaster(new WorldCoordinate(500, 300));

        entity.Location.X.Should().Be(500);
        entity.Location.Y.Should().Be(300);
    }

    [Fact]
    public async Task MoveCaster_WithCasterSelected_ReturnsMoveConfirmationMessage()
    {
        var (service, world) = MakeService();
        var entity = MakeAgencyEntity(x: 0, y: 0);
        world.Add(entity);
        await service.SetCaster(new WorldCoordinate(0, 0));

        var result = await service.MoveCaster(new WorldCoordinate(500, 300));

        result.Text.Should().ContainSingle().Which.Should().Contain("moved");
    }

    [Fact]
    public async Task RegisterInput_NoCasterSelected_ReturnsNoCasterSelectedMessage()
    {
        var (service, _) = MakeService();

        var result = await service.RegisterInput("ZU VUN LA IR HOT IR HOT HOT");

        result.Text.Should().Contain(l => l.Contains("No caster selected"));
    }

    [Fact]
    public async Task SetPointingDirection_NoCasterSelected_ReturnsNoCasterSelectedMessage()
    {
        var (service, _) = MakeService();

        var result = await service.SetPointingDirection(new WorldCoordinate(500, 0));

        result.Text.Should().ContainSingle().Which.Should().Contain("No caster selected");
    }

    [Fact]
    public async Task SetPointingDirection_WithCasterSelected_SetsPointingDirection()
    {
        var (service, world) = MakeService();
        var entity = MakeAgencyEntity(x: 0, y: 0);
        world.Add(entity);
        await service.SetCaster(new WorldCoordinate(0, 0));

        await service.SetPointingDirection(new WorldCoordinate(1000, 0));

        entity.PointingDirection.Should().NotBeNull();
        entity.PointingDirection!.Value.X.Should().BeApproximately(1.0, precision: 0.001);
        entity.PointingDirection!.Value.Y.Should().BeApproximately(0.0, precision: 0.001);
    }

    [Fact]
    public async Task SetPointingDirection_WithCasterSelected_ReturnsConfirmationMessage()
    {
        var (service, world) = MakeService();
        var entity = MakeAgencyEntity(x: 0, y: 0);
        world.Add(entity);
        await service.SetCaster(new WorldCoordinate(0, 0));

        var result = await service.SetPointingDirection(new WorldCoordinate(1000, 0));

        result.Text.Should().ContainSingle().Which.Should().Contain("Pointing direction set");
    }

    [Fact]
    public async Task SetIndicateTarget_NoCasterSelected_ReturnsNoCasterMessage()
    {
        var (service, _) = MakeService();

        var result = await service.SetIndicateTarget(new WorldCoordinate(500, 0));

        result.Text.Should().ContainSingle().Which.Should().Contain("No caster selected");
    }

    [Fact]
    public async Task SetIndicateTarget_NothingAtPoint_ReturnsNothingToIndicateMessage()
    {
        var (service, world) = MakeService();
        var caster = MakeAgencyEntity(x: 0, y: 0);
        world.Add(caster);
        await service.SetCaster(new WorldCoordinate(0, 0));

        var result = await service.SetIndicateTarget(new WorldCoordinate(5000, 5000));

        result.Text.Should().ContainSingle().Which.Should().Contain("Nothing to indicate");
    }

    [Fact]
    public async Task SetIndicateTarget_TargetIsCaster_SetsIndicateTargetToSelfWithNullDirection()
    {
        var (service, world) = MakeService();
        var caster = MakeAgencyEntity(x: 0, y: 0);
        world.Add(caster);
        await service.SetCaster(new WorldCoordinate(0, 0));

        await service.SetIndicateTarget(new WorldCoordinate(0, 0));

        caster.IndicateTarget.Should().NotBeNull();
        caster.IndicateTarget!.EntityId.Should().Be(caster.Id);
        caster.IndicateTarget!.Direction.Should().BeNull();
    }

    [Fact]
    public async Task SetIndicateTarget_TargetIsCaster_ReturnsIndicatingSelfMessage()
    {
        var (service, world) = MakeService();
        var caster = MakeAgencyEntity(x: 0, y: 0);
        world.Add(caster);
        await service.SetCaster(new WorldCoordinate(0, 0));

        var result = await service.SetIndicateTarget(new WorldCoordinate(0, 0));

        result.Text.Should().ContainSingle().Which.Should().Contain("self");
    }

    [Fact]
    public async Task SetIndicateTarget_ObstacleBlocksTarget_ReturnsBlockedMessage()
    {
        var (service, world) = MakeService();
        var caster = MakeAgencyEntity(x: 0, y: 0);
        var obstacle = new EntityBuilder().WithLocation(200, 0).Build();
        var target = new EntityBuilder().WithLocation(500, 0).WithLabel("target").Build();
        world.Add(caster);
        world.Add(obstacle);
        world.Add(target);
        await service.SetCaster(new WorldCoordinate(0, 0));

        var result = await service.SetIndicateTarget(new WorldCoordinate(500, 0));

        result.Text.Should().ContainSingle().Which.Should().Contain("in the way");
    }

    [Fact]
    public async Task SetIndicateTarget_TargetOutOfRange_ReturnsOutOfRangeMessage()
    {
        var (service, world) = MakeService();
        var caster = MakeAgencyEntity(x: 0, y: 0);
        var target = new EntityBuilder().WithLocation(2000, 0).WithLabel("faraway").Build();
        world.Add(caster);
        world.Add(target);
        await service.SetCaster(new WorldCoordinate(0, 0));

        var result = await service.SetIndicateTarget(new WorldCoordinate(2000, 0));

        result.Text.Should().ContainSingle().Which.Should().Contain("out of reach");
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

        caster.IndicateTarget.Should().NotBeNull();
        caster.IndicateTarget!.EntityId.Should().Be(target.Id);
        caster.IndicateTarget!.Direction.Should().NotBeNull();
    }

    [Fact]
    public async Task SetIndicateTarget_TargetInRange_ReturnsIndicatingMessage()
    {
        var (service, world) = MakeService();
        var caster = MakeAgencyEntity(x: 0, y: 0);
        var target = new EntityBuilder().WithLocation(500, 0).WithLabel("target").Build();
        world.Add(caster);
        world.Add(target);
        await service.SetCaster(new WorldCoordinate(0, 0));

        var result = await service.SetIndicateTarget(new WorldCoordinate(500, 0));

        result.Text.Should().ContainSingle().Which.Should().Contain("target");
    }

    [Fact]
    public void Prompt_NoCasterSelected_ReturnsNoCasterPrompt()
    {
        var (service, _) = MakeService();

        service.Prompt.Should().Be("[no caster] >");
    }

    [Fact]
    public async Task Prompt_CasterSelectedWithLife_ShowsHitPoints()
    {
        var (service, world) = MakeService();
        var entity = MakeAgencyEntity(x: 0, y: 0);
        entity.Life = new LifeCapability(maxHitPoints: 20, currentHitPoints: 15);
        world.Add(entity);
        await service.SetCaster(new WorldCoordinate(0, 0));

        service.Prompt.Should().Be("(15/20H) (1000/1000I) >");
    }

    [Fact]
    public async Task Prompt_CasterSelectedWithoutLife_ReturnsDeadCasterPrompt()
    {
        var (service, world) = MakeService();
        var entity = MakeAgencyEntity(x: 0, y: 0);
        entity.Life = null;
        world.Add(entity);
        await service.SetCaster(new WorldCoordinate(0, 0));

        service.Prompt.Should().Be("[dead caster] >");
    }
}
