using FluentAssertions;
using RunicMagic.Tests.Builders;
using RunicMagic.World.Runes.FilterRunes;
using Xunit;

namespace RunicMagic.Tests.Execution.FilterRunes;

public class ZYILTests
{
    [Fact]
    public void Resolve_EntityWithWeightWithinRange_IsReturned()
    {
        var entity = new EntityBuilder().WithWeight(weight: 500).Build();
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
        var entity = new EntityBuilder().WithWeight(weight: 50).Build();
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
        var entity = new EntityBuilder().WithWeight(weight: 2000).Build();
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
        var entity = new EntityBuilder().WithWeight(weight: 100).Build();
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
        var entity = new EntityBuilder().WithWeight(weight: 1000).Build();
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
        var light = new EntityBuilder().WithWeight(weight: 50).Build();
        var medium = new EntityBuilder().WithWeight(weight: 500).Build();
        var heavy = new EntityBuilder().WithWeight(weight: 2000).Build();
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
