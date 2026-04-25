using FluentAssertions;
using RunicMagic.Tests.Builders;
using RunicMagic.World;
using RunicMagic.World.Runes.EntitySetRunes;
using Xunit;

namespace RunicMagic.Tests.Execution.EntitySetRunes;

public class HOROTests
{
    // Origin: 100x100 at (0,0) → bounds [-50,50] on both axes.
    // near:   100x100 at (200,0) → left edge 150, gap to origin right edge 50  = 100
    // far:    100x100 at (500,0) → left edge 450, gap to origin right edge 50  = 400
    private static readonly Entity Origin = new EntityBuilder().WithLocation(x: 0, y: 0).Build();

    private static WorldModel WorldWith(params Entity[] entities)
    {
        var world = new WorldModel();
        foreach (var e in entities)
            world.Add(e);
        return world;
    }

    [Fact]
    public void Resolve_EntityWithinRadius_IsReturned()
    {
        var near = new EntityBuilder().WithLocation(x: 200, y: 0).Build();
        var horo = new HORO(howFar: new FixedNumber(200), origin: new FixedEntitySet(Origin));
        var context = TestFixtures.MakeContext(world: WorldWith(near));

        var result = horo.Resolve(context);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(near);
    }

    [Fact]
    public void Resolve_EntityBeyondRadius_IsNotReturned()
    {
        var far = new EntityBuilder().WithLocation(x: 500, y: 0).Build();
        var horo = new HORO(howFar: new FixedNumber(200), origin: new FixedEntitySet(Origin));
        var context = TestFixtures.MakeContext(world: WorldWith(far));

        var result = horo.Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_EntityExactlyAtRadius_IsReturned()
    {
        var near = new EntityBuilder().WithLocation(x: 200, y: 0).Build();
        var horo = new HORO(howFar: new FixedNumber(100), origin: new FixedEntitySet(Origin));
        var context = TestFixtures.MakeContext(world: WorldWith(near));

        var result = horo.Resolve(context);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(near);
    }

    [Fact]
    public void Resolve_EntityOneUnitBeyondRadius_IsNotReturned()
    {
        var near = new EntityBuilder().WithLocation(x: 200, y: 0).Build();
        var horo = new HORO(howFar: new FixedNumber(99), origin: new FixedEntitySet(Origin));
        var context = TestFixtures.MakeContext(world: WorldWith(near));

        var result = horo.Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_MultipleEntities_ReturnsOnlyThoseWithinRadius()
    {
        var near = new EntityBuilder().WithLocation(x: 200, y: 0).Build();
        var far = new EntityBuilder().WithLocation(x: 500, y: 0).Build();
        var horo = new HORO(howFar: new FixedNumber(200), origin: new FixedEntitySet(Origin));
        var context = TestFixtures.MakeContext(world: WorldWith(near, far));

        var result = horo.Resolve(context);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(near);
    }

    [Fact]
    public void Resolve_EmptyOriginSet_ReturnsNoEntities()
    {
        var near = new EntityBuilder().WithLocation(x: 200, y: 0).Build();
        var horo = new HORO(howFar: new FixedNumber(1000), origin: new FixedEntitySet());
        var context = TestFixtures.MakeContext(world: WorldWith(near));

        var result = horo.Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_EmptyWorld_ReturnsEmpty()
    {
        var horo = new HORO(howFar: new FixedNumber(1000), origin: new FixedEntitySet(Origin));
        var context = TestFixtures.MakeContext(world: new WorldModel());

        var result = horo.Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_WindowOpen_TracksResultEntities()
    {
        var near = new EntityBuilder().WithLocation(x: 200, y: 0).Build();
        var horo = new HORO(howFar: new FixedNumber(200), origin: new FixedEntitySet(Origin));
        var context = TestFixtures.MakeContext(world: WorldWith(near));
        context.OpenResolutionWindow();

        horo.Resolve(context);

        context.EntityResolutionCount.Should().Contain(near.Id);
    }

    [Fact]
    public void Resolve_MultipleOrigins_EntityInRangeOfEachOriginOnly_IsReturned()
    {
        // O1 at (0,0), O2 at (1000,0). nearA is within radius of O1 only (dist=100, gap to O2=700).
        // nearB is within radius of O2 only (dist=100, gap to O1=700). Both qualify via their nearest origin.
        var originO2 = new EntityBuilder().WithLocation(x: 1000, y: 0).Build();
        var nearA = new EntityBuilder().WithLocation(x: 200, y: 0).Build();
        var nearB = new EntityBuilder().WithLocation(x: 800, y: 0).Build();
        var horo = new HORO(howFar: new FixedNumber(200), origin: new FixedEntitySet(Origin, originO2));
        var context = TestFixtures.MakeContext(world: WorldWith(nearA, nearB));

        var result = horo.Resolve(context);

        result.Entities.Should().HaveCount(2);
        result.Entities.Should().Contain(nearA);
        result.Entities.Should().Contain(nearB);
    }
}
