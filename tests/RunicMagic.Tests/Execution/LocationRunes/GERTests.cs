using FluentAssertions;
using RunicMagic.World.Execution;
using RunicMagic.World.Runes.LocationRunes;
using Xunit;

namespace RunicMagic.Tests.Execution.LocationRunes;

public class GERTests
{
    [Fact]
    public void Evaluate_SingleEntity_ReturnsEntityPosition()
    {
        var entity = TestFixtures.MakeEntity(x: 300, y: 500, weight: 10);
        var ger = new GER(new FixedEntitySet(entity));
        var context = TestFixtures.MakeContext();

        var result = ger.Evaluate(context);

        result.Should().Be(new Location(300, 500));
    }

    [Fact]
    public void Evaluate_EqualWeights_ReturnsCentroid()
    {
        var entity1 = TestFixtures.MakeEntity(x: 0, y: 0, weight: 5);
        var entity2 = TestFixtures.MakeEntity(x: 100, y: 200, weight: 5);
        var ger = new GER(new FixedEntitySet(entity1, entity2));
        var context = TestFixtures.MakeContext();

        var result = ger.Evaluate(context);

        result.Should().Be(new Location(50, 100));
    }

    [Fact]
    public void Evaluate_UnequalWeights_PullsTowardHeavierEntity()
    {
        var light = TestFixtures.MakeEntity(x: 0, y: 0, weight: 1);
        var heavy = TestFixtures.MakeEntity(x: 100, y: 0, weight: 3);
        var ger = new GER(new FixedEntitySet(light, heavy));
        var context = TestFixtures.MakeContext();

        var result = ger.Evaluate(context);

        // weighted: (0*1 + 100*3) / 4 = 75
        result.Should().Be(new Location(75, 0));
    }

    [Fact]
    public void Evaluate_AllZeroWeights_FallsBackToUnweightedCentroid()
    {
        var entity1 = TestFixtures.MakeEntity(x: 0, y: 0, weight: 0);
        var entity2 = TestFixtures.MakeEntity(x: 100, y: 0, weight: 0);
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
