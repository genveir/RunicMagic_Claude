using FluentAssertions;
using RunicMagic.Tests.Builders;
using RunicMagic.World.Runes.FilterRunes;
using Xunit;

namespace RunicMagic.Tests.Execution.FilterRunes;

public class ZYHETests
{
    [Fact]
    public void Resolve_SingleEntity_IsReturned()
    {
        var entity = new EntityBuilder().WithWeight(weight: 500).Build();
        var zyhe = new ZYHE(source: new FixedEntitySet(entity));
        var context = TestFixtures.MakeContext();

        var result = zyhe.Resolve(context);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(entity);
    }

    [Fact]
    public void Resolve_MultipleEntities_ReturnsOnlyLightest()
    {
        var light = new EntityBuilder().WithWeight(weight: 100).Build();
        var medium = new EntityBuilder().WithWeight(weight: 500).Build();
        var heavy = new EntityBuilder().WithWeight(weight: 2000).Build();
        var zyhe = new ZYHE(source: new FixedEntitySet(light, medium, heavy));
        var context = TestFixtures.MakeContext();

        var result = zyhe.Resolve(context);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(light);
    }

    [Fact]
    public void Resolve_TiedLightest_ReturnsAllTied()
    {
        var lightA = new EntityBuilder().WithWeight(weight: 100).Build();
        var lightB = new EntityBuilder().WithWeight(weight: 100).Build();
        var heavy = new EntityBuilder().WithWeight(weight: 2000).Build();
        var zyhe = new ZYHE(source: new FixedEntitySet(lightA, lightB, heavy));
        var context = TestFixtures.MakeContext();

        var result = zyhe.Resolve(context);

        result.Entities.Should().HaveCount(2);
        result.Entities.Should().Contain(lightA);
        result.Entities.Should().Contain(lightB);
    }

    [Fact]
    public void Resolve_EmptySource_ReturnsEmpty()
    {
        var zyhe = new ZYHE(source: new FixedEntitySet());
        var context = TestFixtures.MakeContext();

        var result = zyhe.Resolve(context);

        result.Entities.Should().BeEmpty();
    }
}
