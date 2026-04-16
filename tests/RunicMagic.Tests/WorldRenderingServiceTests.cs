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
        var service = new WorldRenderingService(world);

        var result = service.GetAllRenderingModels();

        result.Should().BeEmpty();
    }

    [Fact]
    public void GetAllRenderingModels_ReturnsMappedModelForEachEntity()
    {
        var world = new WorldModel();
        world.Add(MakeEntity("rock", x: 0, y: 0, width: 10, height: 10));
        world.Add(MakeEntity("caster", x: 50, y: 50, width: 20, height: 20,
            hasAgency: true, life: new LifeCapability(100, 100)));
        var service = new WorldRenderingService(world);

        var result = service.GetAllRenderingModels();

        result.Should().HaveCount(2);
        result.Should().Contain(m => m.Label == "rock" && m.Flags == EntityRenderingFlags.None);
        result.Should().Contain(m => m.Label == "caster"
            && m.Flags.HasFlag(EntityRenderingFlags.HasLife)
            && m.Flags.HasFlag(EntityRenderingFlags.HasAgency));
    }

    [Fact]
    public void GetAllRenderingModels_ReflectsCurrentWorldState()
    {
        var world = new WorldModel();
        var service = new WorldRenderingService(world);

        var before = service.GetAllRenderingModels();
        world.Add(MakeEntity("added", x: 0, y: 0, width: 5, height: 5));
        var after = service.GetAllRenderingModels();

        before.Should().BeEmpty();
        after.Should().HaveCount(1);
    }
}
