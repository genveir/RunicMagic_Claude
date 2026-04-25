using FluentAssertions;
using RunicMagic.Tests.Builders;
using RunicMagic.World.Geometry;
using RunicMagic.World.Runes.LocationRunes;
using Xunit;

namespace RunicMagic.Tests.Execution.LocationRunes;

public class GERTests
{
    [Fact]
    public void Evaluate_SingleEntity_ReturnsEntityPosition()
    {
        var entity = new EntityBuilder().WithLocation(x: 300, y: 500).WithWeight(10).Build();
        var ger = new GER(new FixedEntitySet(entity));
        var context = TestFixtures.MakeContext();

        var result = ger.Evaluate(context);

        result.Should().Be(new Location(300, 500));
    }

    [Fact]
    public void Evaluate_EqualWeights_ReturnsCentroid()
    {
        var entity1 = new EntityBuilder().WithLocation(x: 0, y: 0).WithWeight(5).Build();
        var entity2 = new EntityBuilder().WithLocation(x: 100, y: 200).WithWeight(5).Build();
        var ger = new GER(new FixedEntitySet(entity1, entity2));
        var context = TestFixtures.MakeContext();

        var result = ger.Evaluate(context);

        result.Should().Be(new Location(50, 100));
    }

    [Fact]
    public void Evaluate_UnequalWeights_PullsTowardHeavierEntity()
    {
        var light = new EntityBuilder().WithLocation(x: 0, y: 0).WithWeight(1).Build();
        var heavy = new EntityBuilder().WithLocation(x: 100, y: 0).WithWeight(3).Build();
        var ger = new GER(new FixedEntitySet(light, heavy));
        var context = TestFixtures.MakeContext();

        var result = ger.Evaluate(context);

        // weighted: (0*1 + 100*3) / 4 = 75
        result.Should().Be(new Location(75, 0));
    }

    [Fact]
    public void Evaluate_AllZeroWeights_FallsBackToUnweightedCentroid()
    {
        var entity1 = new EntityBuilder().WithLocation(x: 0, y: 0).WithWeight(0).Build();
        var entity2 = new EntityBuilder().WithLocation(x: 100, y: 0).WithWeight(0).Build();
        var ger = new GER(new FixedEntitySet(entity1, entity2));
        var context = TestFixtures.MakeContext();

        var result = ger.Evaluate(context);

        result.Should().Be(new Location(50, 0));
    }

    [Fact]
    public void Evaluate_EmptySet_ReturnsOrigin()
    {
        var ger = new GER(new FixedEntitySet());
        var context = TestFixtures.MakeContext();

        var result = ger.Evaluate(context);

        result.Should().Be(new Location(0, 0));
    }
}
