using FluentAssertions;
using RunicMagic.Controller.Models;
using RunicMagic.Controller.Services;
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

    private static Entity MakeAgencyEntity(long x, long y, string label = "agent") =>
        new(EntityId.New(), EntityType.Object, label)
        {
            Location = new Location(x, y),
            Width = 100,
            Height = 100,
            HasAgency = true,
            Life = new LifeCapability(maxHitPoints: 10, currentHitPoints: 10),
        };

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

        service.Prompt.Should().Be("(15/20) >");
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
