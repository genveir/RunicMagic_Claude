using FluentAssertions;
using RunicMagic.Tests.Builders;
using RunicMagic.World;
using RunicMagic.World.Runes.FilterRunes;
using Xunit;

namespace RunicMagic.Tests.Execution.FilterRunes;

public class FUSETests
{
    private static Entity MakeEntity(long currentPower)
    {
        var entity = new EntityBuilder().Build();
        entity.CurrentReservoir = () => currentPower;
        return entity;
    }

    [Fact]
    public void Resolve_SingleEntity_IsReturned()
    {
        var entity = MakeEntity(currentPower: 500);
        var fuse = new FUSE(source: new FixedEntitySet(entity));
        var context = TestFixtures.MakeContext();

        var result = fuse.Resolve(context);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(entity);
    }

    [Fact]
    public void Resolve_MultipleEntities_ReturnsOnlyMostPowerful()
    {
        var low = MakeEntity(currentPower: 100);
        var mid = MakeEntity(currentPower: 500);
        var high = MakeEntity(currentPower: 2000);
        var fuse = new FUSE(source: new FixedEntitySet(low, mid, high));
        var context = TestFixtures.MakeContext();

        var result = fuse.Resolve(context);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(high);
    }

    [Fact]
    public void Resolve_TiedMostPowerful_ReturnsAllTied()
    {
        var low = MakeEntity(currentPower: 100);
        var highA = MakeEntity(currentPower: 2000);
        var highB = MakeEntity(currentPower: 2000);
        var fuse = new FUSE(source: new FixedEntitySet(low, highA, highB));
        var context = TestFixtures.MakeContext();

        var result = fuse.Resolve(context);

        result.Entities.Should().HaveCount(2);
        result.Entities.Should().Contain(highA);
        result.Entities.Should().Contain(highB);
    }

    [Fact]
    public void Resolve_NullCurrentReservoir_TreatedAsZero()
    {
        var noReservoir = new EntityBuilder().Build();
        var withPower = MakeEntity(currentPower: 500);
        var fuse = new FUSE(source: new FixedEntitySet(noReservoir, withPower));
        var context = TestFixtures.MakeContext();

        var result = fuse.Resolve(context);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(withPower);
    }

    [Fact]
    public void Resolve_EmptySource_ReturnsEmpty()
    {
        var fuse = new FUSE(source: new FixedEntitySet());
        var context = TestFixtures.MakeContext();

        var result = fuse.Resolve(context);

        result.Entities.Should().BeEmpty();
    }
}
