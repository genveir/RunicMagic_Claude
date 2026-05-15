using RunicMagic.World;
using RunicMagic.World.Runes.EntitySetRunes;

namespace RunicMagic.Tests.World.Runes.EntitySetRunes;

public class GATests
{
    [Fact]
    public void Resolve_ReturnsAllWorldEntities()
    {
        var world = new WorldModel();
        var scopeMember = new EntityBuilder().Build();
        var scopeMember2 = new EntityBuilder().WithLocation(x: 100, y: 0).Build();
        world.Add(scopeMember);
        world.Add(scopeMember2);
        var context = TestFixtures.MakeContext(engineAPI: EngineAPIBuilder.ForWorldModel(world).Build());

        var result = new GA().Resolve(context);

        result.Entities.Should().BeEquivalentTo([scopeMember, scopeMember2]);
    }

    [Fact]
    public void Resolve_WindowOpen_AddsExpansionOutputToResolutionCount()
    {
        var world = new WorldModel();

        var scopeMember = new EntityBuilder().Build();
        var scopeMember2 = new EntityBuilder().Build();

        world.Add(scopeMember);
        world.Add(scopeMember2);

        var ga = new GA();
        var context = TestFixtures.MakeContext(engineAPI: EngineAPIBuilder.ForWorldModel(world).Build());
        context.OpenResolutionWindow();

        ga.Resolve(context);

        context.EntityResolutionCount!.EntityIds.Should().Contain(scopeMember.Id);
        context.EntityResolutionCount.EntityIds.Should().Contain(scopeMember2.Id);
    }
}
