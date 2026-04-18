using FluentAssertions;
using RunicMagic.Controller.Models;
using RunicMagic.Controller.Services;
using RunicMagic.World;
using RunicMagic.World.Capabilities;
using Xunit;

namespace RunicMagic.Tests;

public class WorldRenderingServiceTests
{
    private static Entity MakeEntity(string label, int x, int y, int width, int height,
        bool hasAgency = false, LifeCapability? life = null) =>
        new(EntityId.New(), EntityType.Object, label)
        {
            X = x,
            Y = y,
            Width = width,
            Height = height,
            HasAgency = hasAgency,
            Life = life,
        };

    [Fact]
    public void GetAllRenderingModels_ReturnsEmptyList_WhenWorldIsEmpty()
    {
        var world = new WorldModel();
        var service = new WorldRenderingService(world, new RayCastService(world));

        var result = service.GetAllRenderingModels(casterEntityId: null);

        result.Should().BeEmpty();
    }

    [Fact]
    public void GetAllRenderingModels_ReturnsMappedModelForEachEntity()
    {
        var world = new WorldModel();
        world.Add(MakeEntity("rock", x: 0, y: 0, width: 10, height: 10));
        world.Add(MakeEntity("caster", x: 50, y: 50, width: 20, height: 20,
            hasAgency: true, life: new LifeCapability(100, 100)));
        var service = new WorldRenderingService(world, new RayCastService(world));

        var result = service.GetAllRenderingModels(casterEntityId: null);

        result.Should().HaveCount(2);
        result.Should().Contain(m => m.Label == "rock" && m.Flags == EntityRenderingFlags.None);
        result.Should().Contain(m => m.Label == "caster"
            && m.Flags.HasFlag(EntityRenderingFlags.HasLife)
            && m.Flags.HasFlag(EntityRenderingFlags.HasAgency));
    }

    [Fact]
    public void GetAllRenderingModels_MarksEntityWithIsCasterFlag_WhenIdMatches()
    {
        var world = new WorldModel();
        var caster = MakeEntity("caster", x: 0, y: 0, width: 10, height: 10, hasAgency: true);
        var other = MakeEntity("other", x: 50, y: 50, width: 10, height: 10);
        world.Add(caster);
        world.Add(other);
        var service = new WorldRenderingService(world, new RayCastService(world));

        var result = service.GetAllRenderingModels(casterEntityId: caster.Id);

        result.Should().Contain(m => m.Label == "caster" && m.IsCaster);
        result.Should().Contain(m => m.Label == "other" && !m.IsCaster);
    }

    [Fact]
    public void GetAllRenderingModels_EntityWithPointingDirection_IncludesResolvedEndpoint()
    {
        var world = new WorldModel();
        var pointing = new Entity(EntityId.New(), EntityType.Object, "archer")
        {
            X = 0, Y = 0, Width = 100, Height = 100,
            PointingDirection = new RunicMagic.World.Geometry.Direction(1, 0),
        };
        world.Add(pointing);
        var service = new WorldRenderingService(world, new RayCastService(world));

        var result = service.GetAllRenderingModels(casterEntityId: null);

        var model = result.Should().ContainSingle().Subject;
        model.PointingEndX.Should().NotBeNull();
        model.PointingEndY.Should().NotBeNull();
    }

    [Fact]
    public void GetAllRenderingModels_EntityWithoutPointingDirection_HasNullEndpoint()
    {
        var world = new WorldModel();
        world.Add(MakeEntity("rock", x: 0, y: 0, width: 10, height: 10));
        var service = new WorldRenderingService(world, new RayCastService(world));

        var result = service.GetAllRenderingModels(casterEntityId: null);

        result.Should().ContainSingle().Which.PointingEndX.Should().BeNull();
    }

    [Fact]
    public void GetAllRenderingModels_ReflectsCurrentWorldState()
    {
        var world = new WorldModel();
        var service = new WorldRenderingService(world, new RayCastService(world));

        var before = service.GetAllRenderingModels(casterEntityId: null);
        world.Add(MakeEntity("added", x: 0, y: 0, width: 5, height: 5));
        var after = service.GetAllRenderingModels(casterEntityId: null);

        before.Should().BeEmpty();
        after.Should().HaveCount(1);
    }
}
