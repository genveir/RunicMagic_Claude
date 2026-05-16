using RunicMagic.Controller.RuneParsing;
using RunicMagic.Controller.Services;
using RunicMagic.World;
using RunicMagic.World.Engine;
using RunicMagic.World.Entities;
using RunicMagic.World.Entities.Capabilities;
using RunicMagic.World.Execution;
using RunicMagic.World.Motion.Engine;

namespace RunicMagic.Tests.Controller.Services;

public class SpellCastingServiceTests
{
    private static SpellCastingService MakeService(WorldModel world, EngineMotionCollection? engineMotionCollection = null)
    {
        var engineAPI = EngineAPIBuilder.ForWorldModel(world)
            .WithEngineMotionCollection(engineMotionCollection ?? new EngineMotionCollection())
            .Build();
        return new SpellCastingService(new SpellExecutor(world, engineAPI));
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

        service.Cast("", caster, tracker);

        tracker.ParseEvents.Should().ContainSingle().Which.Should().BeOfType<RanOutOfTokensEvent>();
    }

    [Fact]
    public void Cast_UnrecognisedRune_EmitsUnexpectedTokenEvent()
    {
        var world = new WorldModel();
        var caster = AddCaster(world);
        var service = MakeService(world);
        var tracker = new EventTracker();

        service.Cast("NOTARUNE", caster, tracker);

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
        service.Cast("ZU VUN A", caster, tracker);

        tracker.ParseEvents.Should().ContainSingle().Which.Should().BeOfType<RanOutOfTokensEvent>();
    }

    [Fact]
    public void Cast_MilestoneSpell_EmitsEntityPushedEventOnFinalTick()
    {
        var world = new WorldModel();
        var engineMotionCollection = new EngineMotionCollection();
        var ticker = WorldTickerBuilder.ForWorldModel(world).WithEngineMotionCollection(engineMotionCollection).Build();

        var casterEntity = new EntityBuilder()
            .WithLocation(x: 0, y: 0)
            .WithWeight(1)
            .WithReservoir(draw: amount => new ReservoirDraw(amount, false))
            .Build();
        casterEntity.Label = "Caster";

        var target = new EntityBuilder().WithLocation(x: 1000, y: 0).WithWeight(1).Build();
        target.Label = "target";
        casterEntity.Scope = () => new EntitySetSelectionResult(Entities: [target], FictionalResults: 0);

        world.Add(casterEntity);
        world.Add(target);

        var service = MakeService(world, engineMotionCollection);
        service.Cast("ZU VUN LA IR HOT IR HOT HOT", casterEntity, new EventTracker());

        EventTracker finalTick = new EventTracker();
        for (var i = 0; i < Constants.DefaultEffectLength; i++)
        {
            finalTick = new EventTracker();
            ticker.HandleTick(finalTick, i);
        }

        finalTick.WorldEvents.OfType<EntityPushedEvent>().Should().ContainSingle()
            .Which.Entity.Should().BeSameAs(target);
    }

    [Fact]
    public void Cast_MilestoneSpell_EmitsPowerDrawnEvents()
    {
        var world = new WorldModel();
        var engineMotionCollection = new EngineMotionCollection();
        var ticker = WorldTickerBuilder.ForWorldModel(world).WithEngineMotionCollection(engineMotionCollection).Build();

        var casterEntity = new EntityBuilder()
            .WithLocation(x: 0, y: 0)
            .WithWeight(1)
            .WithReservoir(draw: amount => new ReservoirDraw(amount, false))
            .Build();
        casterEntity.Label = "Caster";

        var target = new EntityBuilder().WithLocation(x: 1000, y: 0).WithWeight(1).Build();
        target.Label = "target";
        casterEntity.Scope = () => new EntitySetSelectionResult(Entities: [target], FictionalResults: 0);

        world.Add(casterEntity);
        world.Add(target);

        var service = MakeService(world, engineMotionCollection);
        service.Cast("ZU VUN LA IR HOT IR HOT HOT", casterEntity, new EventTracker());

        var allWorldEvents = new List<WorldEvent>();
        for (var i = 0; i < Constants.DefaultEffectLength; i++)
        {
            var tickTracker = new EventTracker();
            ticker.HandleTick(tickTracker, i);
            allWorldEvents.AddRange(tickTracker.WorldEvents);
        }

        allWorldEvents.OfType<PowerDrawnEvent>().Should().Contain(e => e.Entity.Label == "Caster");
    }
}
