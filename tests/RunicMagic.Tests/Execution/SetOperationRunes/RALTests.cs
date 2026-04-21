using FluentAssertions;
using RunicMagic.World.Runes.SetOperationRunes;
using Xunit;

namespace RunicMagic.Tests.Execution.SetOperationRunes;

public class RALTests
{
    [Fact]
    public void Resolve_EntityOnlyInLeft_IsReturned()
    {
        var leftOnly = TestFixtures.MakeEntity();
        var rightOnly = TestFixtures.MakeEntity();
        var ral = new RAL(left: new FixedEntitySet(leftOnly), right: new FixedEntitySet(rightOnly));
        var context = TestFixtures.MakeContext();

        var result = ral.Resolve(context);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(leftOnly);
    }

    [Fact]
    public void Resolve_EntityInBothSets_IsNotReturned()
    {
        var shared = TestFixtures.MakeEntity();
        var ral = new RAL(left: new FixedEntitySet(shared), right: new FixedEntitySet(shared));
        var context = TestFixtures.MakeContext();

        var result = ral.Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_EntityOnlyInRight_IsNotReturned()
    {
        var rightOnly = TestFixtures.MakeEntity();
        var ral = new RAL(left: new FixedEntitySet(), right: new FixedEntitySet(rightOnly));
        var context = TestFixtures.MakeContext();

        var result = ral.Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_MixedOverlap_ReturnsOnlyLeftExclusive()
    {
        var shared = TestFixtures.MakeEntity();
        var leftOnly = TestFixtures.MakeEntity();
        var rightOnly = TestFixtures.MakeEntity();
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
        var a = TestFixtures.MakeEntity();
        var b = TestFixtures.MakeEntity();
        var ral = new RAL(left: new FixedEntitySet(a, b), right: new FixedEntitySet());
        var context = TestFixtures.MakeContext();

        var result = ral.Resolve(context);

        result.Entities.Should().BeEquivalentTo([a, b]);
    }

    [Fact]
    public void Resolve_LeftEmpty_ReturnsEmpty()
    {
        var right = TestFixtures.MakeEntity();
        var ral = new RAL(left: new FixedEntitySet(), right: new FixedEntitySet(right));
        var context = TestFixtures.MakeContext();

        var result = ral.Resolve(context);

        result.Entities.Should().BeEmpty();
    }
}
