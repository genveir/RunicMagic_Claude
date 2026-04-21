using FluentAssertions;
using RunicMagic.World.Runes.SetOperationRunes;
using Xunit;

namespace RunicMagic.Tests.Execution.SetOperationRunes;

public class DUTests
{
    [Fact]
    public void Resolve_EntityInBothSets_IsReturned()
    {
        var shared = TestFixtures.MakeEntity();
        var du = new DU(left: new FixedEntitySet(shared), right: new FixedEntitySet(shared));
        var context = TestFixtures.MakeContext();

        var result = du.Resolve(context);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(shared);
    }

    [Fact]
    public void Resolve_EntityOnlyInLeft_IsNotReturned()
    {
        var leftOnly = TestFixtures.MakeEntity();
        var rightOnly = TestFixtures.MakeEntity();
        var du = new DU(left: new FixedEntitySet(leftOnly), right: new FixedEntitySet(rightOnly));
        var context = TestFixtures.MakeContext();

        var result = du.Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_EntityOnlyInRight_IsNotReturned()
    {
        var leftOnly = TestFixtures.MakeEntity();
        var rightOnly = TestFixtures.MakeEntity();
        var du = new DU(left: new FixedEntitySet(leftOnly), right: new FixedEntitySet(rightOnly));
        var context = TestFixtures.MakeContext();

        var result = du.Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_MixedOverlap_ReturnsOnlyShared()
    {
        var shared = TestFixtures.MakeEntity();
        var leftOnly = TestFixtures.MakeEntity();
        var rightOnly = TestFixtures.MakeEntity();
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
        var right = TestFixtures.MakeEntity();
        var du = new DU(left: new FixedEntitySet(), right: new FixedEntitySet(right));
        var context = TestFixtures.MakeContext();

        var result = du.Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_RightEmpty_ReturnsEmpty()
    {
        var left = TestFixtures.MakeEntity();
        var du = new DU(left: new FixedEntitySet(left), right: new FixedEntitySet());
        var context = TestFixtures.MakeContext();

        var result = du.Resolve(context);

        result.Entities.Should().BeEmpty();
    }
}
