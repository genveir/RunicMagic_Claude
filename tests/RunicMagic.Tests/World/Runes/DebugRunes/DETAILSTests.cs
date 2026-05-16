using RunicMagic.Controller.Services;
using RunicMagic.World;
using RunicMagic.World.Entities;
using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;
using RunicMagic.World.Runes.DebugRunes;

namespace RunicMagic.Tests.World.Runes.DebugRunes;

public class DETAILSTests
{
    private static readonly Direction Right = new(1, 0);

    private static Entity MakeEntity(long x = 0, long y = 0, long width = 100, long height = 100, string label = "test")
    {
        return new EntityBuilder().WithLabel(label).WithLocation(x, y).WithSize(width, height).Build();
    }

    [Fact]
    public void Execute_NoCaster_EmitsNoCasterEvent()
    {
        var result = new EventTracker();
        var context = TestFixtures.MakeContext(result: result);

        new DETAILS(new FixedEntitySet()).Execute(context);

        result.WorldEvents.OfType<DebugOutputEvent>().Should().ContainSingle()
            .Which.Text.Should().Contain("No caster entity found");
    }

    [Fact]
    public void Execute_WithCaster_EmitsCasterLocationInfo()
    {
        var casterEntity = MakeEntity(x: 100, y: 200, label: "caster");
        var result = new EventTracker();
        var context = TestFixtures.MakeContext(
            caster: new EntitySet([casterEntity]),
            result: result);

        new DETAILS(new FixedEntitySet()).Execute(context);

        result.WorldEvents.OfType<DebugOutputEvent>()
            .Should().Contain(e => e.Text.StartsWith("DETAILS: Caster is caster"));
    }

    [Fact]
    public void Execute_WithCasterAndTargetEntity_EmitsEntityLocationAndDistance()
    {
        var casterEntity = MakeEntity(x: 0, y: 0, label: "caster");
        var target = MakeEntity(x: 500, y: 0, label: "target");
        var result = new EventTracker();
        var context = TestFixtures.MakeContext(
            caster: new EntitySet([casterEntity]),
            result: result);

        new DETAILS(new FixedEntitySet(target)).Execute(context);

        var texts = result.WorldEvents.OfType<DebugOutputEvent>().Select(e => e.Text).ToList();
        texts.Should().Contain(e => e.Contains("target") && e.Contains("location"));
        texts.Should().Contain(e => e.Contains("target") && e.Contains("mm away"));
    }

    [Fact]
    public void Execute_CasterWithNoPointingDirection_DoesNotEmitRayCastEvents()
    {
        var casterEntity = MakeEntity(x: 0, y: 0);
        var result = new EventTracker();
        var context = TestFixtures.MakeContext(
            caster: new EntitySet([casterEntity]),
            result: result);

        new DETAILS(new FixedEntitySet()).Execute(context);

        result.WorldEvents.OfType<DebugOutputEvent>()
            .Should().NotContain(e => e.Text.Contains("pointing at"));
    }

    [Fact]
    public void Execute_CasterPointingAtEntity_EmitsPassedThroughEvent()
    {
        var world = new WorldModel();
        var casterEntity = MakeEntity(x: 0, y: 0, label: "caster");
        casterEntity.PointingDirection = Right;
        var target = MakeEntity(x: 500, y: 0, label: "wall");
        world.Add(casterEntity);
        world.Add(target);
        var result = new EventTracker();
        var context = TestFixtures.MakeContext(
            caster: new EntitySet([casterEntity]),
            engineAPI: EngineAPIBuilder.ForWorldModel(world).Build(),
            result: result);

        new DETAILS(new FixedEntitySet()).Execute(context);

        result.WorldEvents.OfType<DebugOutputEvent>()
            .Should().Contain(e => e.Text == "DETAILS: Ray cast passed through wall.");
    }

    [Fact]
    public void Execute_CasterPointingThroughMultipleEntities_EmitsPassedThroughForEach()
    {
        // DETAILS now reports every entity the ray passes through, not just the first hit.
        var world = new WorldModel();
        var casterEntity = MakeEntity(x: 0, y: 0, label: "caster");
        casterEntity.PointingDirection = Right;
        var near = MakeEntity(x: 500, y: 0, label: "near");
        var far = MakeEntity(x: 1000, y: 0, label: "far");
        world.Add(casterEntity);
        world.Add(near);
        world.Add(far);
        var result = new EventTracker();
        var context = TestFixtures.MakeContext(
            caster: new EntitySet([casterEntity]),
            engineAPI: EngineAPIBuilder.ForWorldModel(world).Build(),
            result: result);

        new DETAILS(new FixedEntitySet()).Execute(context);

        var texts = result.WorldEvents.OfType<DebugOutputEvent>().Select(e => e.Text).ToList();
        texts.Should().Contain("DETAILS: Ray cast passed through near.");
        texts.Should().Contain("DETAILS: Ray cast passed through far.");
    }

    [Fact]
    public void Execute_CasterPointingAtNothing_EmitsNoPassedThroughEvents()
    {
        var world = new WorldModel();
        var casterEntity = MakeEntity(x: 0, y: 0);
        casterEntity.PointingDirection = Right;
        world.Add(casterEntity);
        var result = new EventTracker();
        var context = TestFixtures.MakeContext(
            caster: new EntitySet([casterEntity]),
            engineAPI: EngineAPIBuilder.ForWorldModel(world).Build(),
            result: result);

        new DETAILS(new FixedEntitySet()).Execute(context);

        result.WorldEvents.OfType<DebugOutputEvent>()
            .Should().NotContain(e => e.Text.Contains("passed through"));
    }
}
