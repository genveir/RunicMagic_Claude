using FluentAssertions;
using RunicMagic.Tests.Builders;
using RunicMagic.World.Capabilities;
using RunicMagic.World.Runes.FilterRunes;
using Xunit;

namespace RunicMagic.Tests.Execution.FilterRunes;

public class DRYALTests
{
    [Fact]
    public void Resolve_EntityWithLifeAndPositiveHitPoints_IsReturned()
    {
        var entity = new EntityBuilder().Build();
        entity.Life = new LifeCapability(maxHitPoints: 100, currentHitPoints: 50);
        var dryal = new DRYAL(source: new FixedEntitySet(entity));
        var context = TestFixtures.MakeContext();

        var result = dryal.Resolve(context);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(entity);
    }

    [Fact]
    public void Resolve_EntityWithLifeAndZeroHitPoints_IsNotReturned()
    {
        var entity = new EntityBuilder().Build();
        entity.Life = new LifeCapability(maxHitPoints: 100, currentHitPoints: 0);
        var dryal = new DRYAL(source: new FixedEntitySet(entity));
        var context = TestFixtures.MakeContext();

        var result = dryal.Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_EntityWithNoLifeCapability_IsNotReturned()
    {
        var entity = new EntityBuilder().Build();
        var dryal = new DRYAL(source: new FixedEntitySet(entity));
        var context = TestFixtures.MakeContext();

        var result = dryal.Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_MixedEntities_ReturnsOnlyAlive()
    {
        var alive = new EntityBuilder().WithLife(max: 100, current: 10).Build();
        var dead = new EntityBuilder().WithLife(max: 100, current: 0).Build();
        var inanimate = new EntityBuilder().Build();
        var dryal = new DRYAL(source: new FixedEntitySet(alive, dead, inanimate));
        var context = TestFixtures.MakeContext();

        var result = dryal.Resolve(context);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(alive);
    }

    [Fact]
    public void Resolve_EmptySource_ReturnsEmpty()
    {
        var dryal = new DRYAL(source: new FixedEntitySet());
        var context = TestFixtures.MakeContext();

        var result = dryal.Resolve(context);

        result.Entities.Should().BeEmpty();
    }
}
