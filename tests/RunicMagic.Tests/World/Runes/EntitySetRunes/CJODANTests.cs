using RunicMagic.World;
using RunicMagic.World.Entities;
using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;
using RunicMagic.World.Runes.EntitySetRunes;

namespace RunicMagic.Tests.World.Runes.EntitySetRunes;

public class CJODANTests
{
    // Caster: 100x100 at (0,0), pointing right (1,0).
    // Targets are added to the world alongside the caster.
    private static readonly Direction Right = new(1, 0);

    // TOT=2744 rotation units = one full turn = 360°. DOT=196 ≈ 25.7° half-angle.
    // A very wide cone (TOT/4 = 686 units ≈ 90°) is used for "definitely in" cases.
    // A very narrow cone (HET = 1 unit ≈ 0.13°) is used for "definitely out" cases.
    private static readonly long WideHalfAngle = 686;  // ~90°
    private static readonly long NarrowHalfAngle = 1;  // ~0.13°

    private static Entity MakeCaster()
    {
        var caster = new EntityBuilder().WithLocation(x: 0, y: 0).Build();
        caster.PointingDirection = Right;
        return caster;
    }

    private static (SpellContext context, Entity caster) MakeContextWithCaster(params Entity[] targets)
    {
        var world = new WorldModelBuilder().Build();
        var caster = MakeCaster();
        world.Add(caster);
        foreach (var t in targets)
            world.Add(t);
        var context = TestFixtures.MakeContext(caster: new EntitySet([caster]), world: world);
        return (context, caster);
    }

    [Fact]
    public void Resolve_EmptyCaster_ReturnsEmptySet()
    {
        var context = TestFixtures.MakeContext();

        var result = new CJODAN(range: new FixedNumber(5000), halfAngle: new FixedNumber(WideHalfAngle)).Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_CasterWithNoPointingDirection_ReturnsEmptySet()
    {
        var world = new WorldModelBuilder().Build();
        var casterEntity = new EntityBuilder().WithLocation(x: 0, y: 0).Build();
        world.Add(casterEntity);
        var context = TestFixtures.MakeContext(caster: new EntitySet([casterEntity]), world: world);

        var result = new CJODAN(range: new FixedNumber(5000), halfAngle: new FixedNumber(WideHalfAngle)).Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_EntityDeadAheadWithinRange_IsReturned()
    {
        // Target at (500,0): dead ahead of caster pointing right, clearly within range.
        var target = new EntityBuilder().WithLocation(x: 500, y: 0).Build();
        var (context, _) = MakeContextWithCaster(target);

        var result = new CJODAN(range: new FixedNumber(5000), halfAngle: new FixedNumber(WideHalfAngle)).Resolve(context);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(target);
    }

    [Fact]
    public void Resolve_EntityPerpendicularToPointing_IsNotReturned()
    {
        // Target at (0,500): 90° off the pointing direction, outside any sub-90° half-angle cone.
        var target = new EntityBuilder().WithLocation(x: 0, y: 500).Build();
        var (context, _) = MakeContextWithCaster(target);

        var result = new CJODAN(range: new FixedNumber(5000), halfAngle: new FixedNumber(NarrowHalfAngle)).Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_EntityDeadAheadBeyondRange_IsNotReturned()
    {
        // Target at (5000,0): dead ahead but beyond the 500mm range.
        var target = new EntityBuilder().WithLocation(x: 5000, y: 0).Build();
        var (context, _) = MakeContextWithCaster(target);

        var result = new CJODAN(range: new FixedNumber(500), halfAngle: new FixedNumber(WideHalfAngle)).Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_MixedEntities_ReturnsOnlyThoseInConeAndRange()
    {
        var inCone = new EntityBuilder().WithLocation(x: 500, y: 0).Build();
        var outOfCone = new EntityBuilder().WithLocation(x: 0, y: 500).Build();
        var outOfRange = new EntityBuilder().WithLocation(x: 5000, y: 0).Build();
        var (context, _) = MakeContextWithCaster(inCone, outOfCone, outOfRange);

        var result = new CJODAN(range: new FixedNumber(1000), halfAngle: new FixedNumber(NarrowHalfAngle)).Resolve(context);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(inCone);
    }

    [Fact]
    public void Resolve_DefaultHalfAngle_Dot_IncludesEntityCloseToAxis()
    {
        // DOT = 196 units ≈ 25.7° half-angle. Target slightly off-axis but well within that.
        // Target at (500, 50): angle ≈ atan2(50,500) ≈ 5.7° → inside DOT cone.
        var target = new EntityBuilder().WithLocation(x: 500, y: 50).Build();
        var (context, _) = MakeContextWithCaster(target);

        var result = new CJODAN(range: new FixedNumber(5000), halfAngle: new FixedNumber(196)).Resolve(context);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(target);
    }

    [Fact]
    public void Resolve_WindowOpen_TracksAllResultEntities()
    {
        // targetA dead ahead, targetB offset far enough laterally to avoid occlusion.
        var targetA = new EntityBuilder().WithLocation(x: 500, y: 0).Build();
        var targetB = new EntityBuilder().WithLocation(x: 500, y: 400).Build();
        var (context, _) = MakeContextWithCaster(targetA, targetB);
        context.OpenResolutionWindow();

        new CJODAN(range: new FixedNumber(5000), halfAngle: new FixedNumber(WideHalfAngle)).Resolve(context);

        context.EntityResolutionCount.Should().Contain(targetA.Id);
        context.EntityResolutionCount.Should().Contain(targetB.Id);
    }

    [Fact]
    public void Resolve_WindowOpen_NoEntitiesInCone_ResolutionCountEmpty()
    {
        var target = new EntityBuilder().WithLocation(x: 0, y: 500).Build();
        var (context, _) = MakeContextWithCaster(target);
        context.OpenResolutionWindow();

        new CJODAN(range: new FixedNumber(5000), halfAngle: new FixedNumber(NarrowHalfAngle)).Resolve(context);

        context.EntityResolutionCount.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_OccludingEntityBlocksEntityBehindIt()
    {
        // front at (300,0) blocks back at (700,0) — both dead ahead, front is opaque.
        var front = new EntityBuilder().WithLocation(x: 300, y: 0).Build();
        var back = new EntityBuilder().WithLocation(x: 700, y: 0).Build();
        var (context, _) = MakeContextWithCaster(front, back);

        var result = new CJODAN(range: new FixedNumber(5000), halfAngle: new FixedNumber(WideHalfAngle)).Resolve(context);

        result.Entities.Should().Contain(front);
        result.Entities.Should().NotContain(back);
    }
}
