using RunicMagic.Tests.Builders;
using RunicMagic.World;
using RunicMagic.World.Entities;
using RunicMagic.World.Geometry;

namespace RunicMagic.Tests.World.Geometry;

public class ConeCastServiceTests
{
    private static readonly Direction Right = new(1, 0);
    private const double Wide = Math.PI / 2;   // 90-degree half-angle
    private const double Narrow = Math.PI / 8; // 22.5-degree half-angle

    private static Entity MakeEntity(long x, long y, long width = 100, long height = 100, bool isTranslucent = false)
    {
        var builder = new EntityBuilder().WithLocation(x, y).WithSize(width, height);
        if (isTranslucent) builder.WithTranslucency();
        return builder.Build();
    }

    [Fact]
    public void Cast_NoEntities_ReturnsEmpty()
    {
        var world = new WorldModelBuilder().Build();
        var origin = MakeEntity(x: 0, y: 0);
        world.Add(origin);
        var service = new ConeCastService(world);

        var result = service.Cast(origin, Right, halfAngle: Wide, range: 2000);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Cast_SkipsOriginEntity()
    {
        var world = new WorldModelBuilder().Build();
        var origin = MakeEntity(x: 0, y: 0);
        world.Add(origin);
        var service = new ConeCastService(world);

        var result = service.Cast(origin, Right, halfAngle: Wide, range: 2000);

        result.Should().NotContain(origin);
    }

    [Fact]
    public void Cast_EntityInCone_IsReturned()
    {
        var world = new WorldModelBuilder().Build();
        var origin = MakeEntity(x: 0, y: 0);
        var target = MakeEntity(x: 500, y: 0);
        world.Add(origin);
        world.Add(target);
        var service = new ConeCastService(world);

        var result = service.Cast(origin, Right, halfAngle: Wide, range: 2000);

        result.Should().Contain(target);
    }

    [Fact]
    public void Cast_EntityBehindOrigin_IsNotReturned()
    {
        var world = new WorldModelBuilder().Build();
        var origin = MakeEntity(x: 0, y: 0);
        var behind = MakeEntity(x: -500, y: 0);
        world.Add(origin);
        world.Add(behind);
        var service = new ConeCastService(world);

        var result = service.Cast(origin, Right, halfAngle: Wide, range: 2000);

        result.Should().NotContain(behind);
    }

    [Fact]
    public void Cast_EntityOutsideRange_IsNotReturned()
    {
        var world = new WorldModelBuilder().Build();
        var origin = MakeEntity(x: 0, y: 0);
        var far = MakeEntity(x: 3000, y: 0);
        world.Add(origin);
        world.Add(far);
        var service = new ConeCastService(world);

        var result = service.Cast(origin, Right, halfAngle: Wide, range: 500);

        result.Should().NotContain(far);
    }

    [Fact]
    public void Cast_EntityOutsideAngle_IsNotReturned()
    {
        var world = new WorldModelBuilder().Build();
        var origin = MakeEntity(x: 0, y: 0);
        // Directly perpendicular — well outside a narrow forward cone
        var side = MakeEntity(x: 0, y: 500);
        world.Add(origin);
        world.Add(side);
        var service = new ConeCastService(world);

        var result = service.Cast(origin, Right, halfAngle: Narrow, range: 2000);

        result.Should().NotContain(side);
    }

    [Fact]
    public void Cast_OccludedEntity_IsNotReturned()
    {
        var world = new WorldModelBuilder().Build();
        var origin = MakeEntity(x: 0, y: 0);
        var blocker = MakeEntity(x: 500, y: 0, width: 400, height: 400);
        var hidden = MakeEntity(x: 1000, y: 0);
        world.Add(origin);
        world.Add(blocker);
        world.Add(hidden);
        var service = new ConeCastService(world);

        var result = service.Cast(origin, Right, halfAngle: Wide, range: 2000);

        result.Should().Contain(blocker);
        result.Should().NotContain(hidden);
    }

    [Fact]
    public void Cast_PartiallyVisibleEntity_IsReturned()
    {
        // Origin at 0,0 facing right. A wide wall at x=1000 extends from y=-600 to y=600.
        // Two pillars at x=500, y=+/-200 occlude the centre and near-corners of the wall,
        // but leave its outer edges visible.
        var world = new WorldModelBuilder().Build();
        var origin = MakeEntity(x: 0, y: 0);
        var wall = MakeEntity(x: 1000, y: 0, width: 100, height: 1200);
        var pillarTop = MakeEntity(x: 500, y: 200, width: 100, height: 200);
        var pillarBottom = MakeEntity(x: 500, y: -200, width: 100, height: 200);
        world.Add(origin);
        world.Add(wall);
        world.Add(pillarTop);
        world.Add(pillarBottom);
        var service = new ConeCastService(world);

        var result = service.Cast(origin, Right, halfAngle: Wide, range: 2000);

        result.Should().Contain(wall);
    }

    [Fact]
    public void Cast_TranslucentOccluderDoesNotBlockTarget()
    {
        var world = new WorldModelBuilder().Build();
        var origin = MakeEntity(x: 0, y: 0);
        var glass = MakeEntity(x: 500, y: 0, width: 400, height: 400, isTranslucent: true);
        var target = MakeEntity(x: 1000, y: 0);
        world.Add(origin);
        world.Add(glass);
        world.Add(target);
        var service = new ConeCastService(world);

        var result = service.Cast(origin, Right, halfAngle: Wide, range: 2000);

        result.Should().Contain(target);
    }

    [Fact]
    public void Cast_TranslucentEntityIncludedInResults_WhenNotOccluded()
    {
        var world = new WorldModelBuilder().Build();
        var origin = MakeEntity(x: 0, y: 0);
        var glass = MakeEntity(x: 500, y: 0, isTranslucent: true);
        world.Add(origin);
        world.Add(glass);
        var service = new ConeCastService(world);

        // skipTranslucent: false — translucent entities are candidates
        var result = service.Cast(origin, Right, halfAngle: Wide, range: 2000, skipTranslucent: false);

        result.Should().Contain(glass);
    }

    [Fact]
    public void Cast_SkipTranslucentTrue_TranslucentEntityExcluded()
    {
        var world = new WorldModelBuilder().Build();
        var origin = MakeEntity(x: 0, y: 0);
        var glass = MakeEntity(x: 500, y: 0, isTranslucent: true);
        world.Add(origin);
        world.Add(glass);
        var service = new ConeCastService(world);

        var result = service.Cast(origin, Right, halfAngle: Wide, range: 2000, skipTranslucent: true);

        result.Should().NotContain(glass);
    }
}
