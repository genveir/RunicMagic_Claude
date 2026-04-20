using FluentAssertions;
using RunicMagic.World.Capabilities;
using RunicMagic.World.Runes.FilterRunes;
using Xunit;

namespace RunicMagic.Tests.Execution.FilterRunes;

public class DRYALTests
{
    [Fact]
    public void Resolve_EntityWithLifeAndPositiveHitPoints_IsReturned()
    {
        var entity = TestFixtures.MakeEntity();
        entity.Life = new LifeCapability(maxHitPoints: 100, currentHitPoints: 50);
        var dryal = new DRYAL(source: new FixedEntitySet(entity));
        var context = TestFixtures.MakeContext();

        var result = dryal.Resolve(context);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(entity);
    }

    [Fact]
    public void Resolve_EntityWithLifeAndZeroHitPoints_IsNotReturned()
    {
        var entity = TestFixtures.MakeEntity();
        entity.Life = new LifeCapability(maxHitPoints: 100, currentHitPoints: 0);
        var dryal = new DRYAL(source: new FixedEntitySet(entity));
        var context = TestFixtures.MakeContext();

        var result = dryal.Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_EntityWithNoLifeCapability_IsNotReturned()
    {
        var entity = TestFixtures.MakeEntity();
        var dryal = new DRYAL(source: new FixedEntitySet(entity));
        var context = TestFixtures.MakeContext();

        var result = dryal.Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_MixedEntities_ReturnsOnlyAlive()
    {
        var alive = TestFixtures.MakeEntity();
        alive.Life = new LifeCapability(maxHitPoints: 100, currentHitPoints: 10);
        var dead = TestFixtures.MakeEntity();
        dead.Life = new LifeCapability(maxHitPoints: 100, currentHitPoints: 0);
        var inanimate = TestFixtures.MakeEntity();
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
