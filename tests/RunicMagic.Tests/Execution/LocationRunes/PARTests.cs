using FluentAssertions;
using RunicMagic.World.Execution;
using RunicMagic.World.Runes.LocationRunes;
using Xunit;

namespace RunicMagic.Tests.Execution.LocationRunes;

public class PARTests
{
    [Fact]
    public void Evaluate_SingleEntity_ReturnsEntityPosition()
    {
        var entity = TestFixtures.MakeEntity(x: 300, y: 500);
        var par = new PAR(new FixedEntitySet(entity));
        var context = TestFixtures.MakeContext();

        var result = par.Evaluate(context);

        result.X.Should().Be(300);
        result.Y.Should().Be(500);
    }

    [Fact]
    public void Evaluate_MultipleEntities_ReturnsCentroid()
    {
        var entity1 = TestFixtures.MakeEntity(x: 0, y: 0);
        var entity2 = TestFixtures.MakeEntity(x: 100, y: 200);
        var par = new PAR(new FixedEntitySet(entity1, entity2));
        var context = TestFixtures.MakeContext();

        var result = par.Evaluate(context);

        result.X.Should().Be(50);
        result.Y.Should().Be(100);
    }

    [Fact]
    public void Evaluate_EmptySet_ReturnsOrigin()
    {
        var par = new PAR(new FixedEntitySet());
        var context = TestFixtures.MakeContext();

        var result = par.Evaluate(context);

        result.X.Should().Be(0);
        result.Y.Should().Be(0);
    }
}
