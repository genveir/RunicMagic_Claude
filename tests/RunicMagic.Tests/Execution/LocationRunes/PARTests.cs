using RunicMagic.World.Execution;
using RunicMagic.World.Runes.LocationRunes;

namespace RunicMagic.Tests.Execution.LocationRunes;

public class PARTests
{
    [Fact]
    public void Evaluate_SingleEntity_ReturnsEntityPosition()
    {
        var entity = new EntityBuilder().WithLocation(x: 300, y: 500).Build();
        var par = new PAR(new FixedEntitySet(entity));
        var context = TestFixtures.MakeContext();

        var result = par.Evaluate(context);

        result.X.Should().Be(300);
        result.Y.Should().Be(500);
    }

    [Fact]
    public void Evaluate_MultipleEntities_ReturnsCentroid()
    {
        var entity1 = new EntityBuilder().WithLocation(x: 0, y: 0).Build();
        var entity2 = new EntityBuilder().WithLocation(x: 100, y: 200).Build();
        var par = new PAR(new FixedEntitySet(entity1, entity2));
        var context = TestFixtures.MakeContext();

        var result = par.Evaluate(context);

        result.X.Should().Be(50);
        result.Y.Should().Be(100);
    }

    [Fact]
    public void Evaluate_EmptySet_ReturnsExecutorLocation()
    {
        var executorEntity = new EntityBuilder().WithLocation(x: 200, y: 400).Build();
        var par = new PAR(new FixedEntitySet());
        var context = TestFixtures.MakeContext(executor: new EntitySet([executorEntity]));

        var result = par.Evaluate(context);

        result.X.Should().Be(200);
        result.Y.Should().Be(400);
    }
}
