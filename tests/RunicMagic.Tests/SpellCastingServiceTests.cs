using RunicMagic.Controller.Models;
using RunicMagic.Controller.RuneParsing;
using RunicMagic.Controller.Services;
using RunicMagic.World;
using RunicMagic.World.Capabilities;
using RunicMagic.World.Execution;

namespace RunicMagic.Tests;

public class SpellCastingServiceTests
{
    private static SpellCastingService MakeService(WorldModel world)
    {
        return new SpellCastingService(world, new SpellExecutor(world));
    }

    private static Entity AddCaster(WorldModel world)
    {
        var caster = new EntityBuilder().WithLocation(x: 0, y: 0).WithWeight(1).Build();
        world.Add(caster);
        return caster;
    }

    [Fact]
    public void Cast_EmptyInput_EmitsRanOutOfTokensEvent()
    {
        var world = new WorldModel();
        var caster = AddCaster(world);
        var service = MakeService(world);
        var tracker = new EventTracker();

        service.Cast("", caster.Id, tracker);

        tracker.ParseEvents.Should().ContainSingle().Which.Should().BeOfType<RanOutOfTokensEvent>();
    }

    [Fact]
    public void Cast_UnrecognisedRune_EmitsUnexpectedTokenEvent()
    {
        var world = new WorldModel();
        var caster = AddCaster(world);
        var service = MakeService(world);
        var tracker = new EventTracker();

        service.Cast("NOTARUNE", caster.Id, tracker);

        tracker.ParseEvents.Should().ContainSingle().Which.Should().BeOfType<UnexpectedTokenEvent>()
            .Which.Token.Should().Be("NOTARUNE");
    }

    [Fact]
    public void Cast_IncompleteSpell_EmitsRanOutOfTokensEvent()
    {
        var world = new WorldModel();
        var caster = AddCaster(world);
        var service = MakeService(world);
        var tracker = new EventTracker();

        // ZU VUN A — missing Number argument for VUN
        service.Cast("ZU VUN A", caster.Id, tracker);

        tracker.ParseEvents.Should().ContainSingle().Which.Should().BeOfType<RanOutOfTokensEvent>();
    }

    [Fact]
    public void Cast_ValidSpell_NoCasterSelected_EmitsNoCasterSelectedEvent()
    {
        var world = new WorldModel();
        var service = MakeService(world);
        var tracker = new EventTracker();

        service.Cast("ZU VUN LA IR HOT IR HOT HOT", casterId: null, tracker);

        tracker.ControllerEvents.Should().ContainSingle().Which.Should().BeOfType<NoCasterSelectedEvent>();
    }

    [Fact]
    public void Cast_ValidSpell_CasterIdNotInWorld_EmitsCasterNotFoundEvent()
    {
        var world = new WorldModel();
        var service = MakeService(world);
        var tracker = new EventTracker();

        service.Cast("ZU VUN LA IR HOT IR HOT HOT", EntityId.New(), tracker);

        tracker.ControllerEvents.Should().ContainSingle().Which.Should().BeOfType<CasterNotFoundEvent>();
    }

    [Fact]
    public void Cast_MilestoneSpell_EmitsEntityPushedEventOnFinalTick()
    {
        var world = new WorldModel();

        var casterEntity = new EntityBuilder()
            .WithLocation(x: 0, y: 0)
            .WithWeight(1)
            .WithReservoir(draw: amount => new ReservoirDraw(amount, false))
            .Build();
        casterEntity.Label = "Caster";

        var target = new EntityBuilder().WithLocation(x: 1000, y: 0).WithWeight(1).Build();
        target.Label = "target";
        casterEntity.Scope = () => [target];

        world.Add(casterEntity);
        world.Add(target);

        var service = MakeService(world);
        service.Cast("ZU VUN LA IR HOT IR HOT HOT", casterEntity.Id, new EventTracker());

        EventTracker finalTick = new EventTracker();
        for (var i = 0; i < 56; i++)
        {
            finalTick = new EventTracker();
            world.HandleTick(finalTick);
        }

        finalTick.WorldEvents.OfType<EntityPushedEvent>().Should().ContainSingle()
            .Which.Entity.Should().BeSameAs(target);
    }

    [Fact]
    public void Cast_MilestoneSpell_EmitsPowerDrawnEvents()
    {
        var world = new WorldModel();

        var casterEntity = new EntityBuilder()
            .WithLocation(x: 0, y: 0)
            .WithWeight(1)
            .WithReservoir(draw: amount => new ReservoirDraw(amount, false))
            .Build();
        casterEntity.Label = "Caster";

        var target = new EntityBuilder().WithLocation(x: 1000, y: 0).WithWeight(1).Build();
        target.Label = "target";
        casterEntity.Scope = () => [target];

        world.Add(casterEntity);
        world.Add(target);

        var service = MakeService(world);
        service.Cast("ZU VUN LA IR HOT IR HOT HOT", casterEntity.Id, new EventTracker());

        var allWorldEvents = new List<WorldEvent>();
        for (var i = 0; i < 56; i++)
        {
            var tickTracker = new EventTracker();
            world.HandleTick(tickTracker);
            allWorldEvents.AddRange(tickTracker.WorldEvents);
        }

        allWorldEvents.OfType<PowerDrawnEvent>().Should().Contain(e => e.Entity.Label == "Caster");
    }
}
