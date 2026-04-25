using FluentAssertions;
using RunicMagic.Tests.Builders;
using RunicMagic.World.Runes.SetOperationRunes;
using Xunit;

namespace RunicMagic.Tests.Execution.SetOperationRunes;

public class ANTests
{
    [Fact]
    public void Resolve_ReturnsEntitiesFromBothSets()
    {
        var left = new EntityBuilder().Build();
        var right = new EntityBuilder().Build();
        var an = new AN(left: new FixedEntitySet(left), right: new FixedEntitySet(right));
        var context = TestFixtures.MakeContext();

        var result = an.Resolve(context);

        result.Entities.Should().BeEquivalentTo([left, right]);
    }

    [Fact]
    public void Resolve_EntityInBothSets_IsReturnedOnce()
    {
        var shared = new EntityBuilder().Build();
        var an = new AN(left: new FixedEntitySet(shared), right: new FixedEntitySet(shared));
        var context = TestFixtures.MakeContext();

        var result = an.Resolve(context);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(shared);
    }

    [Fact]
    public void Resolve_LeftEmpty_ReturnsRight()
    {
        var right = new EntityBuilder().Build();
        var an = new AN(left: new FixedEntitySet(), right: new FixedEntitySet(right));
        var context = TestFixtures.MakeContext();

        var result = an.Resolve(context);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(right);
    }

    [Fact]
    public void Resolve_RightEmpty_ReturnsLeft()
    {
        var left = new EntityBuilder().Build();
        var an = new AN(left: new FixedEntitySet(left), right: new FixedEntitySet());
        var context = TestFixtures.MakeContext();

        var result = an.Resolve(context);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(left);
    }

    [Fact]
    public void Resolve_BothEmpty_ReturnsEmpty()
    {
        var an = new AN(left: new FixedEntitySet(), right: new FixedEntitySet());
        var context = TestFixtures.MakeContext();

        var result = an.Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_PreservesLeftOrderBeforeRight()
    {
        var a = new EntityBuilder().Build();
        var b = new EntityBuilder().Build();
        var c = new EntityBuilder().Build();
        var an = new AN(left: new FixedEntitySet(a, b), right: new FixedEntitySet(c));
        var context = TestFixtures.MakeContext();

        var result = an.Resolve(context);

        result.Entities.Should().Equal([a, b, c]);
    }
}
