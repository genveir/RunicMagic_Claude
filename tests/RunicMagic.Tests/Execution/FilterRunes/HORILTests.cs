using FluentAssertions;
using RunicMagic.World;
using RunicMagic.World.Runes.FilterRunes;
using Xunit;

namespace RunicMagic.Tests.Execution.FilterRunes;

public class HORILTests
{
    // Origin: 100x100 at (0,0) → bounds [-50,50] on both axes.
    // near:   100x100 at (200,0)  → surface gap to origin = 100
    // medium: 100x100 at (350,0)  → surface gap to origin = 250
    // far:    100x100 at (500,0)  → surface gap to origin = 400
    private static readonly Entity Origin = TestFixtures.MakeEntity(x: 0, y: 0);

    [Fact]
    public void Resolve_EntityWithDistanceInRange_IsReturned()
    {
        var near = TestFixtures.MakeEntity(x: 200, y: 0);
        var horil = new HORIL(
            source: new FixedEntitySet(near),
            lower: new FixedNumber(50),
            upper: new FixedNumber(200),
            origin: new FixedEntitySet(Origin));
        var context = TestFixtures.MakeContext();

        var result = horil.Resolve(context);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(near);
    }

    [Fact]
    public void Resolve_EntityWithDistanceBelowLower_IsNotReturned()
    {
        var near = TestFixtures.MakeEntity(x: 200, y: 0);
        var horil = new HORIL(
            source: new FixedEntitySet(near),
            lower: new FixedNumber(200),
            upper: new FixedNumber(500),
            origin: new FixedEntitySet(Origin));
        var context = TestFixtures.MakeContext();

        var result = horil.Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_EntityWithDistanceAboveUpper_IsNotReturned()
    {
        var far = TestFixtures.MakeEntity(x: 500, y: 0);
        var horil = new HORIL(
            source: new FixedEntitySet(far),
            lower: new FixedNumber(50),
            upper: new FixedNumber(200),
            origin: new FixedEntitySet(Origin));
        var context = TestFixtures.MakeContext();

        var result = horil.Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_EntityWithDistanceEqualToLower_IsNotReturned()
    {
        var near = TestFixtures.MakeEntity(x: 200, y: 0);
        var horil = new HORIL(
            source: new FixedEntitySet(near),
            lower: new FixedNumber(100),
            upper: new FixedNumber(200),
            origin: new FixedEntitySet(Origin));
        var context = TestFixtures.MakeContext();

        var result = horil.Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_EntityWithDistanceEqualToUpper_IsNotReturned()
    {
        var near = TestFixtures.MakeEntity(x: 200, y: 0);
        var horil = new HORIL(
            source: new FixedEntitySet(near),
            lower: new FixedNumber(50),
            upper: new FixedNumber(100),
            origin: new FixedEntitySet(Origin));
        var context = TestFixtures.MakeContext();

        var result = horil.Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_MultipleEntities_ReturnsOnlyThoseInRange()
    {
        var near = TestFixtures.MakeEntity(x: 200, y: 0);
        var medium = TestFixtures.MakeEntity(x: 350, y: 0);
        var far = TestFixtures.MakeEntity(x: 500, y: 0);
        var horil = new HORIL(
            source: new FixedEntitySet(near, medium, far),
            lower: new FixedNumber(50),
            upper: new FixedNumber(300),
            origin: new FixedEntitySet(Origin));
        var context = TestFixtures.MakeContext();

        var result = horil.Resolve(context);

        result.Entities.Should().HaveCount(2);
        result.Entities.Should().Contain(near);
        result.Entities.Should().Contain(medium);
    }

    [Fact]
    public void Resolve_EmptySource_ReturnsEmpty()
    {
        var horil = new HORIL(
            source: new FixedEntitySet(),
            lower: new FixedNumber(50),
            upper: new FixedNumber(200),
            origin: new FixedEntitySet(Origin));
        var context = TestFixtures.MakeContext();

        var result = horil.Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_MultipleOrigins_EffectiveDistanceIsToNearestOriginEntity()
    {
        // O1 at (0,0), O2 at (800,0).
        // entityA at (200,0): dist to O1=100, dist to O2=500 → effective dist=100 → in range (50,200)
        // entityB at (600,0): dist to O1=500, dist to O2=100 → effective dist=100 → in range (50,200)
        // entityC at (1200,0): dist to O1=1100, dist to O2=300 → effective dist=300 → out of range
        var originO2 = TestFixtures.MakeEntity(x: 800, y: 0);
        var entityA = TestFixtures.MakeEntity(x: 200, y: 0);
        var entityB = TestFixtures.MakeEntity(x: 600, y: 0);
        var entityC = TestFixtures.MakeEntity(x: 1200, y: 0);
        var horil = new HORIL(
            source: new FixedEntitySet(entityA, entityB, entityC),
            lower: new FixedNumber(50),
            upper: new FixedNumber(200),
            origin: new FixedEntitySet(Origin, originO2));
        var context = TestFixtures.MakeContext();

        var result = horil.Resolve(context);

        result.Entities.Should().HaveCount(2);
        result.Entities.Should().Contain(entityA);
        result.Entities.Should().Contain(entityB);
    }
}
