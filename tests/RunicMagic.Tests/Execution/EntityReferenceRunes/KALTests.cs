using FluentAssertions;
using RunicMagic.World;
using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;
using RunicMagic.World.Runes.EntityReferenceRunes;
using Xunit;

namespace RunicMagic.Tests.Execution.EntityReferenceRunes;

public class KALTests
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

        var result = new KAL().Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_CasterWithNoIndicateTarget_ReturnsEmptySet()
    {
        var casterEntity = MakeEntity(x: 0, y: 0);
        var context = TestFixtures.MakeContext(
            caster: new EntitySet([casterEntity]),
            world: new WorldModel());

        var result = new KAL().Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_SelfIndicate_ReturnsCaster()
    {
        var world = new WorldModel();
        var casterEntity = MakeEntity(x: 0, y: 0);
        casterEntity.IndicateTarget = new IndicateTarget(casterEntity.Id, Direction: null);
        world.Add(casterEntity);
        var context = TestFixtures.MakeContext(
            caster: new EntitySet([casterEntity]),
            world: world);

        var result = new KAL().Resolve(context);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(casterEntity);
    }

    [Fact]
    public void Resolve_IndicateTargetInRange_ReturnsSingletonWithThatEntity()
    {
        var world = new WorldModel();
        var casterEntity = MakeEntity(x: 0, y: 0);
        var target = MakeEntity(x: 500, y: 0);
        casterEntity.IndicateTarget = new IndicateTarget(target.Id, Right);
        world.Add(casterEntity);
        world.Add(target);
        var context = TestFixtures.MakeContext(
            caster: new EntitySet([casterEntity]),
            world: world);

        var result = new KAL().Resolve(context);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(target);
    }

    [Fact]
    public void Resolve_TranslucentEntityBlocksPath_ReturnsEmptySet()
    {
        var world = new WorldModel();
        var casterEntity = MakeEntity(x: 0, y: 0);
        var glass = new Entity(EntityId.New(), EntityType.Object, "glass")
        {
            X = 300,
            Y = 0,
            Width = 100,
            Height = 100,
            IsTranslucent = true,
        };
        var target = MakeEntity(x: 600, y: 0);
        casterEntity.IndicateTarget = new IndicateTarget(target.Id, Right);
        world.Add(casterEntity);
        world.Add(glass);
        world.Add(target);
        var context = TestFixtures.MakeContext(
            caster: new EntitySet([casterEntity]),
            world: world);

        var result = new KAL().Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_IndicateTargetEntityDestroyed_ReturnsEmptySet()
    {
        var world = new WorldModel();
        var casterEntity = MakeEntity(x: 0, y: 0);
        var destroyedId = EntityId.New();
        casterEntity.IndicateTarget = new IndicateTarget(destroyedId, Right);
        world.Add(casterEntity);
        var context = TestFixtures.MakeContext(
            caster: new EntitySet([casterEntity]),
            world: world);

        var result = new KAL().Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_TargetMovedOutOfRange_ReturnsEmptySet()
    {
        var world = new WorldModel();
        var casterEntity = MakeEntity(x: 0, y: 0);
        var target = MakeEntity(x: 1200, y: 0);
        casterEntity.IndicateTarget = new IndicateTarget(target.Id, Right);
        world.Add(casterEntity);
        world.Add(target);
        var context = TestFixtures.MakeContext(
            caster: new EntitySet([casterEntity]),
            world: world);

        var result = new KAL().Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_OpaqueEntityBlocksPath_ReturnsEmptySet()
    {
        var world = new WorldModel();
        var casterEntity = MakeEntity(x: 0, y: 0);
        var wall = MakeEntity(x: 300, y: 0);
        var target = MakeEntity(x: 600, y: 0);
        casterEntity.IndicateTarget = new IndicateTarget(target.Id, Right);
        world.Add(casterEntity);
        world.Add(wall);
        world.Add(target);
        var context = TestFixtures.MakeContext(
            caster: new EntitySet([casterEntity]),
            world: world);

        var result = new KAL().Resolve(context);

        result.Entities.Should().BeEmpty();
    }
}
