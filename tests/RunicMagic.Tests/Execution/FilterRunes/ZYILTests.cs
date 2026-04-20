using FluentAssertions;
using RunicMagic.World.Runes.FilterRunes;
using Xunit;

namespace RunicMagic.Tests.Execution.FilterRunes;

public class ZYILTests
{
    [Fact]
    public void Resolve_EntityWithWeightWithinRange_IsReturned()
    {
        var entity = TestFixtures.MakeEntity(weight: 500);
        var zyil = new ZYIL(
            source: new FixedEntitySet(entity),
            lower: new FixedNumber(100),
            upper: new FixedNumber(1000));
        var context = TestFixtures.MakeContext();

        var result = zyil.Resolve(context);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(entity);
    }

    [Fact]
    public void Resolve_EntityWithWeightBelowLower_IsNotReturned()
    {
        var entity = TestFixtures.MakeEntity(weight: 50);
        var zyil = new ZYIL(
            source: new FixedEntitySet(entity),
            lower: new FixedNumber(100),
            upper: new FixedNumber(1000));
        var context = TestFixtures.MakeContext();

        var result = zyil.Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_EntityWithWeightAboveUpper_IsNotReturned()
    {
        var entity = TestFixtures.MakeEntity(weight: 2000);
        var zyil = new ZYIL(
            source: new FixedEntitySet(entity),
            lower: new FixedNumber(100),
            upper: new FixedNumber(1000));
        var context = TestFixtures.MakeContext();

        var result = zyil.Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_EntityWithWeightEqualToLower_IsNotReturned()
    {
        var entity = TestFixtures.MakeEntity(weight: 100);
        var zyil = new ZYIL(
            source: new FixedEntitySet(entity),
            lower: new FixedNumber(100),
            upper: new FixedNumber(1000));
        var context = TestFixtures.MakeContext();

        var result = zyil.Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_EntityWithWeightEqualToUpper_IsNotReturned()
    {
        var entity = TestFixtures.MakeEntity(weight: 1000);
        var zyil = new ZYIL(
            source: new FixedEntitySet(entity),
            lower: new FixedNumber(100),
            upper: new FixedNumber(1000));
        var context = TestFixtures.MakeContext();

        var result = zyil.Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_MultipleEntities_ReturnsOnlyThoseInRange()
    {
        var light = TestFixtures.MakeEntity(weight: 50);
        var medium = TestFixtures.MakeEntity(weight: 500);
        var heavy = TestFixtures.MakeEntity(weight: 2000);
        var zyil = new ZYIL(
            source: new FixedEntitySet(light, medium, heavy),
            lower: new FixedNumber(100),
            upper: new FixedNumber(1000));
        var context = TestFixtures.MakeContext();

        var result = zyil.Resolve(context);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(medium);
    }

    [Fact]
    public void Resolve_EmptySource_ReturnsEmpty()
    {
        var zyil = new ZYIL(
            source: new FixedEntitySet(),
            lower: new FixedNumber(100),
            upper: new FixedNumber(1000));
        var context = TestFixtures.MakeContext();

        var result = zyil.Resolve(context);

        result.Entities.Should().BeEmpty();
    }
}
