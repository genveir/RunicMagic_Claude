using FluentAssertions;
using RunicMagic.World;
using RunicMagic.World.Runes.FilterRunes;
using Xunit;

namespace RunicMagic.Tests.Execution.FilterRunes;

public class FUILTests
{
    private static Entity MakeEntity(long currentPower)
    {
        var entity = TestFixtures.MakeEntity();
        entity.CurrentReservoir = () => currentPower;
        return entity;
    }

    [Fact]
    public void Resolve_EntityWithPowerWithinRange_IsReturned()
    {
        var entity = MakeEntity(currentPower: 500);
        var fuil = new FUIL(
            source: new FixedEntitySet(entity),
            lower: new FixedNumber(100),
            upper: new FixedNumber(1000));
        var context = TestFixtures.MakeContext();

        var result = fuil.Resolve(context);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(entity);
    }

    [Fact]
    public void Resolve_EntityWithPowerBelowLower_IsNotReturned()
    {
        var entity = MakeEntity(currentPower: 50);
        var fuil = new FUIL(
            source: new FixedEntitySet(entity),
            lower: new FixedNumber(100),
            upper: new FixedNumber(1000));
        var context = TestFixtures.MakeContext();

        var result = fuil.Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_EntityWithPowerAboveUpper_IsNotReturned()
    {
        var entity = MakeEntity(currentPower: 2000);
        var fuil = new FUIL(
            source: new FixedEntitySet(entity),
            lower: new FixedNumber(100),
            upper: new FixedNumber(1000));
        var context = TestFixtures.MakeContext();

        var result = fuil.Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_EntityWithPowerEqualToLower_IsNotReturned()
    {
        var entity = MakeEntity(currentPower: 100);
        var fuil = new FUIL(
            source: new FixedEntitySet(entity),
            lower: new FixedNumber(100),
            upper: new FixedNumber(1000));
        var context = TestFixtures.MakeContext();

        var result = fuil.Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_EntityWithPowerEqualToUpper_IsNotReturned()
    {
        var entity = MakeEntity(currentPower: 1000);
        var fuil = new FUIL(
            source: new FixedEntitySet(entity),
            lower: new FixedNumber(100),
            upper: new FixedNumber(1000));
        var context = TestFixtures.MakeContext();

        var result = fuil.Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_EntityWithNullCurrentReservoir_TreatedAsZero()
    {
        var entity = TestFixtures.MakeEntity();
        var fuil = new FUIL(
            source: new FixedEntitySet(entity),
            lower: new FixedNumber(-1),
            upper: new FixedNumber(1));
        var context = TestFixtures.MakeContext();

        var result = fuil.Resolve(context);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(entity);
    }

    [Fact]
    public void Resolve_MultipleEntities_ReturnsOnlyThoseInRange()
    {
        var low = MakeEntity(currentPower: 50);
        var mid = MakeEntity(currentPower: 500);
        var high = MakeEntity(currentPower: 2000);
        var fuil = new FUIL(
            source: new FixedEntitySet(low, mid, high),
            lower: new FixedNumber(100),
            upper: new FixedNumber(1000));
        var context = TestFixtures.MakeContext();

        var result = fuil.Resolve(context);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(mid);
    }

    [Fact]
    public void Resolve_EmptySource_ReturnsEmpty()
    {
        var fuil = new FUIL(
            source: new FixedEntitySet(),
            lower: new FixedNumber(100),
            upper: new FixedNumber(1000));
        var context = TestFixtures.MakeContext();

        var result = fuil.Resolve(context);

        result.Entities.Should().BeEmpty();
    }
}
