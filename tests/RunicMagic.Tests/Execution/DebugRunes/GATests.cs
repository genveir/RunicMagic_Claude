using FluentAssertions;
using RunicMagic.Tests.Builders;
using RunicMagic.World;
using RunicMagic.World.Capabilities;
using RunicMagic.World.Execution;
using RunicMagic.World.Runes.DebugRunes;
using Xunit;

namespace RunicMagic.Tests.Execution.DebugRunes;

public class GATests
{
    [Fact]
    public void Resolve_ReturnsAllWorldEntities()
    {
        var world = new WorldModel();
        var entity1 = new EntityBuilder().Build();
        var entity2 = new EntityBuilder().WithLocation(x: 100, y: 0).Build();
        world.Add(entity1);
        world.Add(entity2);
        var context = TestFixtures.MakeContext(world: world);

        var result = new GA().Resolve(context);

        result.Entities.Should().BeEquivalentTo([entity1, entity2]);
    }

    [Fact]
    public void Resolve_WithNoResolutionWindow_DoesNotEmitDebugEvent()
    {
        var world = new WorldModel();
        world.Add(new EntityBuilder().Build());
        var spellResult = new SpellResult();
        var context = TestFixtures.MakeContext(world: world, result: spellResult);

        new GA().Resolve(context);

        spellResult.Events.OfType<DebugOutputEvent>().Should().BeEmpty();
    }

    [Fact]
    public void Resolve_WithNoResolutionWindow_DoesNotDrawPower()
    {
        var drawn = new List<long>();
        var casterEntity = new EntityBuilder()
            .WithReservoir(draw: amount => { drawn.Add(amount); return new ReservoirDraw(amount, false); })
            .Build();
        var world = new WorldModel();
        world.Add(new EntityBuilder().Build());
        var context = TestFixtures.MakeContext(
            caster: new EntitySet([casterEntity]),
            world: world);

        new GA().Resolve(context);

        drawn.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_WithOpenResolutionWindow_EmitsDebugEvent()
    {
        var world = new WorldModel();
        var spellResult = new SpellResult();
        var context = TestFixtures.MakeContext(world: world, result: spellResult);
        context.OpenResolutionWindow();

        new GA().Resolve(context);

        spellResult.Events.OfType<DebugOutputEvent>().Should().ContainSingle()
            .Which.Text.Should().Contain("burned out your fragile mortal soul");
    }

    [Fact]
    public void Resolve_WithOpenResolutionWindow_DrawsOneBillionPower()
    {
        var drawn = new List<long>();
        var casterEntity = new EntityBuilder()
            .WithReservoir(draw: amount => { drawn.Add(amount); return new ReservoirDraw(amount, false); })
            .Build();
        var world = new WorldModel();
        var context = TestFixtures.MakeContext(
            caster: new EntitySet([casterEntity]),
            world: world);
        context.OpenResolutionWindow();

        new GA().Resolve(context);

        drawn.Should().ContainSingle().Which.Should().Be(1_000_000_000);
    }

    [Fact]
    public void Resolve_WithOpenResolutionWindow_StillReturnsAllWorldEntities()
    {
        var world = new WorldModel();
        var entity = new EntityBuilder().Build();
        world.Add(entity);
        var context = TestFixtures.MakeContext(world: world);
        context.OpenResolutionWindow();

        var result = new GA().Resolve(context);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(entity);
    }
}
