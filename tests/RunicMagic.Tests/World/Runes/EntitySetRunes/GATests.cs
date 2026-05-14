using RunicMagic.World.Runes.EntitySetRunes;

namespace RunicMagic.Tests.World.Runes.EntitySetRunes;

public class GATests
{
    [Fact]
    public void Resolve_ReturnsAllWorldEntities()
    {
        var world = new WorldModelBuilder().Build();
        var scopeMember = new EntityBuilder().Build();
        var scopeMember2 = new EntityBuilder().WithLocation(x: 100, y: 0).Build();
        world.Add(scopeMember);
        world.Add(scopeMember2);
        var context = TestFixtures.MakeContext(world: world);

        var result = new GA().Resolve(context);

        result.Entities.Should().BeEquivalentTo([scopeMember, scopeMember2]);
    }

    [Fact]
    public void Resolve_WindowOpen_AddsExpansionOutputToResolutionCount()
    {
        var world = new WorldModelBuilder().Build();

        var scopeMember = new EntityBuilder().Build();
        var scopeMember2 = new EntityBuilder().Build();

        world.Add(scopeMember);
        world.Add(scopeMember2);

        var ga = new GA();
        var context = TestFixtures.MakeContext(world: world);
        context.OpenResolutionWindow();

        ga.Resolve(context);

        context.EntityResolutionCount.Should().Contain(scopeMember.Id);
        context.EntityResolutionCount.Should().Contain(scopeMember2.Id);
    }
}
