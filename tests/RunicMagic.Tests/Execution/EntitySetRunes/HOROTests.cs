using FluentAssertions;
using RunicMagic.World;
using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;
using RunicMagic.World.Runes.EntitySetRunes;
using Xunit;

namespace RunicMagic.Tests.Execution.EntitySetRunes;

public class HOROTests
{
    private static Entity MakeEntity(long x, long y, long width = 100, long height = 100)
    {
        return new Entity(EntityId.New(), EntityType.Object, "test")
        {
            Location = new Location(x, y),
            Width = width,
            Height = height,
        };
    }

    [Fact]
    public void Resolve_EntityWithinRadius_IsReturned()
    {
        var world = new WorldModel();
        var entity = MakeEntity(x: 500, y: 0);
        world.Add(entity);

        var horo = new HORO(
            howFar: new FixedNumber(600),
            origin: new FixedLocation(0, 0));
        var context = TestFixtures.MakeContext(world: world);

        var result = horo.Resolve(context);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(entity);
    }

    [Fact]
    public void Resolve_EntityOutsideRadius_IsNotReturned()
    {
        var world = new WorldModel();
        var entity = MakeEntity(x: 1000, y: 0);
        world.Add(entity);

        var horo = new HORO(
            howFar: new FixedNumber(400),
            origin: new FixedLocation(0, 0));
        var context = TestFixtures.MakeContext(world: world);

        var result = horo.Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_EntityAtExactBoundary_IsReturned()
    {
        var world = new WorldModel();
        // Entity centre at (550, 0), width 100 → nearest edge at x=500
        var entity = MakeEntity(x: 550, y: 0);
        world.Add(entity);

        var horo = new HORO(
            howFar: new FixedNumber(500),
            origin: new FixedLocation(0, 0));
        var context = TestFixtures.MakeContext(world: world);

        var result = horo.Resolve(context);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(entity);
    }

    [Fact]
    public void Resolve_UsesOriginFromLocation()
    {
        var world = new WorldModel();
        // Entity 200mm from origin at (1000, 0), so nearest edge at (950, 0) → 50mm from origin
        var entity = MakeEntity(x: 1000, y: 0);
        world.Add(entity);

        var horo = new HORO(
            howFar: new FixedNumber(100),
            origin: new FixedLocation(900, 0));
        var context = TestFixtures.MakeContext(world: world);

        var result = horo.Resolve(context);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(entity);
    }

    [Fact]
    public void Resolve_MultipleEntities_ReturnsOnlyThoseWithinRadius()
    {
        var world = new WorldModel();
        var near = MakeEntity(x: 200, y: 0);
        var far = MakeEntity(x: 2000, y: 0);
        world.Add(near);
        world.Add(far);

        var horo = new HORO(
            howFar: new FixedNumber(500),
            origin: new FixedLocation(0, 0));
        var context = TestFixtures.MakeContext(world: world);

        var result = horo.Resolve(context);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(near);
    }

    [Fact]
    public void Resolve_NoEntitiesInWorld_ReturnsEmptySet()
    {
        var world = new WorldModel();
        var horo = new HORO(
            howFar: new FixedNumber(1000),
            origin: new FixedLocation(0, 0));
        var context = TestFixtures.MakeContext(world: world);

        var result = horo.Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_LargeEntityBridgesRadius_IsIncluded()
    {
        var world = new WorldModel();
        // Entity centre at (2000, 0), width 4000 → nearest edge at x=0, distance 0 from origin
        var largeEntity = MakeEntity(x: 2000, y: 0, width: 4000, height: 100);
        world.Add(largeEntity);

        var horo = new HORO(
            howFar: new FixedNumber(1),
            origin: new FixedLocation(0, 0));
        var context = TestFixtures.MakeContext(world: world);

        var result = horo.Resolve(context);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(largeEntity);
    }
}
