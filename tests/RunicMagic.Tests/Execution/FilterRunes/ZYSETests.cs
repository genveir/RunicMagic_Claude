using FluentAssertions;
using RunicMagic.Tests.Builders;
using RunicMagic.World.Runes.FilterRunes;
using Xunit;

namespace RunicMagic.Tests.Execution.FilterRunes;

public class ZYSETests
{
    [Fact]
    public void Resolve_SingleEntity_IsReturned()
    {
        var entity = new EntityBuilder().WithWeight(weight: 500).Build();
        var zyse = new ZYSE(source: new FixedEntitySet(entity));
        var context = TestFixtures.MakeContext();

        var result = zyse.Resolve(context);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(entity);
    }

    [Fact]
    public void Resolve_MultipleEntities_ReturnsOnlyHeaviest()
    {
        var light = new EntityBuilder().WithWeight(weight: 100).Build();
        var medium = new EntityBuilder().WithWeight(weight: 500).Build();
        var heavy = new EntityBuilder().WithWeight(weight: 2000).Build();
        var zyse = new ZYSE(source: new FixedEntitySet(light, medium, heavy));
        var context = TestFixtures.MakeContext();

        var result = zyse.Resolve(context);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(heavy);
    }

    [Fact]
    public void Resolve_TiedHeaviest_ReturnsAllTied()
    {
        var light = new EntityBuilder().WithWeight(weight: 100).Build();
        var heavyA = new EntityBuilder().WithWeight(weight: 2000).Build();
        var heavyB = new EntityBuilder().WithWeight(weight: 2000).Build();
        var zyse = new ZYSE(source: new FixedEntitySet(light, heavyA, heavyB));
        var context = TestFixtures.MakeContext();

        var result = zyse.Resolve(context);

        result.Entities.Should().HaveCount(2);
        result.Entities.Should().Contain(heavyA);
        result.Entities.Should().Contain(heavyB);
    }

    [Fact]
    public void Resolve_EmptySource_ReturnsEmpty()
    {
        var zyse = new ZYSE(source: new FixedEntitySet());
        var context = TestFixtures.MakeContext();

        var result = zyse.Resolve(context);

        result.Entities.Should().BeEmpty();
    }
}
