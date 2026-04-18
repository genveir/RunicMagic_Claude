using FluentAssertions;
using RunicMagic.World;
using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;
using RunicMagic.World.Runes.EntityReferenceRunes;
using Xunit;

namespace RunicMagic.Tests.Execution.EntityReferenceRunes;

public class DANTests
{
    private static readonly Direction Right = new(1, 0);

    private static Entity MakeEntity(long x, long y, long width = 100, long height = 100)
    {
        return new Entity(EntityId.New(), EntityType.Object, "test")
        {
            X = x,
            Y = y,
            Width = width,
            Height = height,
        };
    }

    [Fact]
    public void Resolve_EmptyCaster_ReturnsEmptySet()
    {
        var context = TestFixtures.MakeContext();

        var result = new DAN().Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_CasterWithNoPointingDirection_ReturnsEmptySet()
    {
        var casterEntity = MakeEntity(x: 0, y: 0);
        var context = TestFixtures.MakeContext(
            caster: new EntitySet([casterEntity]),
            world: new WorldModel());

        var result = new DAN().Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_CasterPointingAtNothing_ReturnsEmptySet()
    {
        var world = new WorldModel();
        var casterEntity = MakeEntity(x: 0, y: 0);
        casterEntity.PointingDirection = Right;
        world.Add(casterEntity);
        var context = TestFixtures.MakeContext(
            caster: new EntitySet([casterEntity]),
            world: world);

        var result = new DAN().Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_CasterPointingAtEntity_ReturnsSingletonWithThatEntity()
    {
        var world = new WorldModel();
        var casterEntity = MakeEntity(x: 0, y: 0);
        casterEntity.PointingDirection = Right;
        var target = MakeEntity(x: 500, y: 0);
        world.Add(casterEntity);
        world.Add(target);
        var context = TestFixtures.MakeContext(
            caster: new EntitySet([casterEntity]),
            world: world);

        var result = new DAN().Resolve(context);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(target);
    }

    [Fact]
    public void Resolve_TranslucentEntityInPath_IsNotReturned()
    {
        var world = new WorldModel();
        var casterEntity = MakeEntity(x: 0, y: 0);
        casterEntity.PointingDirection = Right;
        var glass = new Entity(EntityId.New(), EntityType.Object, "glass")
        {
            X = 300,
            Y = 0,
            Width = 100,
            Height = 100,
            IsTranslucent = true,
        };
        world.Add(casterEntity);
        world.Add(glass);
        var context = TestFixtures.MakeContext(
            caster: new EntitySet([casterEntity]),
            world: world);

        var result = new DAN().Resolve(context);

        result.Entities.Should().BeEmpty();
    }
}
