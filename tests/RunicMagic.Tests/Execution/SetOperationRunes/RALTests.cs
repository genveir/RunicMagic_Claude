using FluentAssertions;
using RunicMagic.Tests.Builders;
using RunicMagic.World.Runes.SetOperationRunes;
using Xunit;

namespace RunicMagic.Tests.Execution.SetOperationRunes;

public class RALTests
{
    [Fact]
    public void Resolve_EntityOnlyInLeft_IsReturned()
    {
        var leftOnly = new EntityBuilder().Build();
        var rightOnly = new EntityBuilder().Build();
        var ral = new RAL(left: new FixedEntitySet(leftOnly), right: new FixedEntitySet(rightOnly));
        var context = TestFixtures.MakeContext();

        var result = ral.Resolve(context);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(leftOnly);
    }

    [Fact]
    public void Resolve_EntityInBothSets_IsNotReturned()
    {
        var shared = new EntityBuilder().Build();
        var ral = new RAL(left: new FixedEntitySet(shared), right: new FixedEntitySet(shared));
        var context = TestFixtures.MakeContext();

        var result = ral.Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_EntityOnlyInRight_IsNotReturned()
    {
        var rightOnly = new EntityBuilder().Build();
        var ral = new RAL(left: new FixedEntitySet(), right: new FixedEntitySet(rightOnly));
        var context = TestFixtures.MakeContext();

        var result = ral.Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_MixedOverlap_ReturnsOnlyLeftExclusive()
    {
        var shared = new EntityBuilder().Build();
        var leftOnly = new EntityBuilder().Build();
        var rightOnly = new EntityBuilder().Build();
        var ral = new RAL(
            left: new FixedEntitySet(shared, leftOnly),
            right: new FixedEntitySet(shared, rightOnly));
        var context = TestFixtures.MakeContext();

        var result = ral.Resolve(context);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(leftOnly);
    }

    [Fact]
    public void Resolve_RightEmpty_ReturnsAllOfLeft()
    {
        var a = new EntityBuilder().Build();
        var b = new EntityBuilder().Build();
        var ral = new RAL(left: new FixedEntitySet(a, b), right: new FixedEntitySet());
        var context = TestFixtures.MakeContext();

        var result = ral.Resolve(context);

        result.Entities.Should().BeEquivalentTo([a, b]);
    }

    [Fact]
    public void Resolve_LeftEmpty_ReturnsEmpty()
    {
        var right = new EntityBuilder().Build();
        var ral = new RAL(left: new FixedEntitySet(), right: new FixedEntitySet(right));
        var context = TestFixtures.MakeContext();

        var result = ral.Resolve(context);

        result.Entities.Should().BeEmpty();
    }
}
