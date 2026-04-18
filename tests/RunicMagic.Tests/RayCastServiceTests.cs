using FluentAssertions;
using RunicMagic.World;
using RunicMagic.World.Geometry;
using Xunit;

namespace RunicMagic.Tests;

public class RayCastServiceTests
{
    private static Entity MakeEntity(int x, int y, int width = 100, int height = 100, bool isTranslucent = false)
    {
        return new Entity(EntityId.New(), EntityType.Object, "test")
        {
            X = x,
            Y = y,
            Width = width,
            Height = height,
            IsTranslucent = isTranslucent,
        };
    }

    private static readonly Direction Right = new(1, 0);

    [Fact]
    public void Cast_NoEntities_ReturnsFallbackPoint()
    {
        var world = new WorldModel();
        var service = new RayCastService(world);
        var sourceId = EntityId.New();

        var (x, y) = service.Cast(sourceId, originX: 0, originY: 0, Right);

        x.Should().Be(3000);
        y.Should().Be(0);
    }

    [Fact]
    public void Cast_SkipsSourceEntity()
    {
        var world = new WorldModel();
        var source = MakeEntity(x: 0, y: 0);
        world.Add(source);
        var service = new RayCastService(world);

        // If source were not skipped, the ray would start inside it and be rejected by
        // IntersectsRay, but it should be excluded by ID before geometry is checked.
        var (x, _) = service.Cast(source.Id, originX: 0, originY: 0, Right);

        x.Should().Be(3000);
    }

    [Fact]
    public void Cast_TranslucentEntityInPath_IsIgnored()
    {
        var world = new WorldModel();
        var source = MakeEntity(x: 0, y: 0);
        var ice = MakeEntity(x: 500, y: 0, isTranslucent: true);
        world.Add(source);
        world.Add(ice);
        var service = new RayCastService(world);

        var (x, _) = service.Cast(source.Id, originX: 0, originY: 0, Right);

        x.Should().Be(3000);
    }

    [Fact]
    public void Cast_NonTranslucentEntityInPath_ReturnsEntryPoint()
    {
        var world = new WorldModel();
        var source = MakeEntity(x: 0, y: 0);
        var wall = MakeEntity(x: 500, y: 0);
        world.Add(source);
        world.Add(wall);
        var service = new RayCastService(world);

        var (x, _) = service.Cast(source.Id, originX: 0, originY: 0, Right);

        x.Should().Be(450); // left edge of wall at x=500, width=100
    }

    [Fact]
    public void Cast_MultipleEntities_ReturnsClosestHit()
    {
        var world = new WorldModel();
        var source = MakeEntity(x: 0, y: 0);
        var near = MakeEntity(x: 500, y: 0);
        var far  = MakeEntity(x: 1000, y: 0);
        world.Add(source);
        world.Add(near);
        world.Add(far);
        var service = new RayCastService(world);

        var (x, _) = service.Cast(source.Id, originX: 0, originY: 0, Right);

        x.Should().Be(450); // near wall entry, not far
    }

    [Fact]
    public void Cast_TranslucentBeforeNonTranslucent_ReturnsNonTranslucentHit()
    {
        var world = new WorldModel();
        var source = MakeEntity(x: 0, y: 0);
        var ice  = MakeEntity(x: 300, y: 0, isTranslucent: true);
        var wall = MakeEntity(x: 700, y: 0);
        world.Add(source);
        world.Add(ice);
        world.Add(wall);
        var service = new RayCastService(world);

        var (x, _) = service.Cast(source.Id, originX: 0, originY: 0, Right);

        x.Should().Be(650); // wall entry, ice skipped
    }
}
