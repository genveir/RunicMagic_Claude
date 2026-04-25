using FluentAssertions;
using RunicMagic.Tests.Builders;
using RunicMagic.World.Runes.SetOperationRunes;
using Xunit;

namespace RunicMagic.Tests.Execution.SetOperationRunes;

public class DUTests
{
    [Fact]
    public void Resolve_EntityInBothSets_IsReturned()
    {
        var shared = new EntityBuilder().Build();
        var du = new DU(left: new FixedEntitySet(shared), right: new FixedEntitySet(shared));
        var context = TestFixtures.MakeContext();

        var result = du.Resolve(context);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(shared);
    }

    [Fact]
    public void Resolve_EntityOnlyInLeft_IsNotReturned()
    {
        var leftOnly = new EntityBuilder().Build();
        var rightOnly = new EntityBuilder().Build();
        var du = new DU(left: new FixedEntitySet(leftOnly), right: new FixedEntitySet(rightOnly));
        var context = TestFixtures.MakeContext();

        var result = du.Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_EntityOnlyInRight_IsNotReturned()
    {
        var leftOnly = new EntityBuilder().Build();
        var rightOnly = new EntityBuilder().Build();
        var du = new DU(left: new FixedEntitySet(leftOnly), right: new FixedEntitySet(rightOnly));
        var context = TestFixtures.MakeContext();

        var result = du.Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_MixedOverlap_ReturnsOnlyShared()
    {
        var shared = new EntityBuilder().Build();
        var leftOnly = new EntityBuilder().Build();
        var rightOnly = new EntityBuilder().Build();
        var du = new DU(
            left: new FixedEntitySet(shared, leftOnly),
            right: new FixedEntitySet(shared, rightOnly));
        var context = TestFixtures.MakeContext();

        var result = du.Resolve(context);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(shared);
    }

    [Fact]
    public void Resolve_LeftEmpty_ReturnsEmpty()
    {
        var right = new EntityBuilder().Build();
        var du = new DU(left: new FixedEntitySet(), right: new FixedEntitySet(right));
        var context = TestFixtures.MakeContext();

        var result = du.Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_RightEmpty_ReturnsEmpty()
    {
        var left = new EntityBuilder().Build();
        var du = new DU(left: new FixedEntitySet(left), right: new FixedEntitySet());
        var context = TestFixtures.MakeContext();

        var result = du.Resolve(context);

        result.Entities.Should().BeEmpty();
    }
}
