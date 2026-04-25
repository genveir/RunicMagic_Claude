using FluentAssertions;
using RunicMagic.Tests.Builders;
using RunicMagic.World;
using RunicMagic.World.Runes.FilterRunes;
using Xunit;

namespace RunicMagic.Tests.Execution.FilterRunes;

public class FUHETests
{
    private static Entity MakeEntity(long currentPower)
    {
        var entity = new EntityBuilder()
            .WithReservoir(current: () => currentPower)
            .Build();
        return entity;
    }

    [Fact]
    public void Resolve_SingleEntity_IsReturned()
    {
        var entity = MakeEntity(currentPower: 500);
        var fuhe = new FUHE(source: new FixedEntitySet(entity));
        var context = TestFixtures.MakeContext();

        var result = fuhe.Resolve(context);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(entity);
    }

    [Fact]
    public void Resolve_MultipleEntities_ReturnsOnlyLeastPowerful()
    {
        var low = MakeEntity(currentPower: 100);
        var mid = MakeEntity(currentPower: 500);
        var high = MakeEntity(currentPower: 2000);
        var fuhe = new FUHE(source: new FixedEntitySet(low, mid, high));
        var context = TestFixtures.MakeContext();

        var result = fuhe.Resolve(context);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(low);
    }

    [Fact]
    public void Resolve_TiedLeastPowerful_ReturnsAllTied()
    {
        var lowA = MakeEntity(currentPower: 100);
        var lowB = MakeEntity(currentPower: 100);
        var high = MakeEntity(currentPower: 2000);
        var fuhe = new FUHE(source: new FixedEntitySet(lowA, lowB, high));
        var context = TestFixtures.MakeContext();

        var result = fuhe.Resolve(context);

        result.Entities.Should().HaveCount(2);
        result.Entities.Should().Contain(lowA);
        result.Entities.Should().Contain(lowB);
    }

    [Fact]
    public void Resolve_NullCurrentReservoir_TreatedAsZero()
    {
        var noReservoir = new EntityBuilder().Build();
        var withPower = MakeEntity(currentPower: 500);
        var fuhe = new FUHE(source: new FixedEntitySet(noReservoir, withPower));
        var context = TestFixtures.MakeContext();

        var result = fuhe.Resolve(context);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(noReservoir);
    }

    [Fact]
    public void Resolve_EmptySource_ReturnsEmpty()
    {
        var fuhe = new FUHE(source: new FixedEntitySet());
        var context = TestFixtures.MakeContext();

        var result = fuhe.Resolve(context);

        result.Entities.Should().BeEmpty();
    }
}
