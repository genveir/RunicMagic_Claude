using FluentAssertions;
using RunicMagic.World;
using RunicMagic.World.Runes.FilterRunes;
using Xunit;

namespace RunicMagic.Tests.Execution.FilterRunes;

public class HORHETests
{
    // Origin: 100x100 at (0,0) → bounds [-50,50] on both axes.
    // near:   100x100 at (200,0)  → surface gap to origin = 100
    // far:    100x100 at (500,0)  → surface gap to origin = 400
    // mirror: 100x100 at (-200,0) → surface gap to origin = 100  (symmetric twin of near)
    private static readonly Entity Origin = TestFixtures.MakeEntity(x: 0, y: 0);

    [Fact]
    public void Resolve_SingleEntity_IsReturned()
    {
        var near = TestFixtures.MakeEntity(x: 200, y: 0);
        var horhe = new HORHE(source: new FixedEntitySet(near), origin: new FixedEntitySet(Origin));
        var context = TestFixtures.MakeContext();

        var result = horhe.Resolve(context);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(near);
    }

    [Fact]
    public void Resolve_MultipleEntities_ReturnsOnlyClosest()
    {
        var near = TestFixtures.MakeEntity(x: 200, y: 0);
        var far = TestFixtures.MakeEntity(x: 500, y: 0);
        var horhe = new HORHE(source: new FixedEntitySet(near, far), origin: new FixedEntitySet(Origin));
        var context = TestFixtures.MakeContext();

        var result = horhe.Resolve(context);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(near);
    }

    [Fact]
    public void Resolve_TiedClosest_ReturnsAllTied()
    {
        var left = TestFixtures.MakeEntity(x: -200, y: 0);
        var right = TestFixtures.MakeEntity(x: 200, y: 0);
        var far = TestFixtures.MakeEntity(x: 500, y: 0);
        var horhe = new HORHE(source: new FixedEntitySet(left, right, far), origin: new FixedEntitySet(Origin));
        var context = TestFixtures.MakeContext();

        var result = horhe.Resolve(context);

        result.Entities.Should().HaveCount(2);
        result.Entities.Should().Contain(left);
        result.Entities.Should().Contain(right);
    }

    [Fact]
    public void Resolve_EmptySource_ReturnsEmpty()
    {
        var horhe = new HORHE(source: new FixedEntitySet(), origin: new FixedEntitySet(Origin));
        var context = TestFixtures.MakeContext();

        var result = horhe.Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_EmptyOrigin_ReturnsAllSource()
    {
        // All source entities have distance +∞ to an empty origin set, so all tie as "closest".
        var near = TestFixtures.MakeEntity(x: 200, y: 0);
        var far = TestFixtures.MakeEntity(x: 500, y: 0);
        var horhe = new HORHE(source: new FixedEntitySet(near, far), origin: new FixedEntitySet());
        var context = TestFixtures.MakeContext();

        var result = horhe.Resolve(context);

        result.Entities.Should().HaveCount(2);
        result.Entities.Should().Contain(near);
        result.Entities.Should().Contain(far);
    }

    [Fact]
    public void Resolve_MultipleOrigins_DistanceIsToNearestOriginEntity()
    {
        // O1 at (0,0), O2 at (800,0).
        // entityA at (200,0): dist to O1=100, dist to O2=500 → effective dist=100
        // entityB at (600,0): dist to O1=500, dist to O2=100 → effective dist=100
        // entityC at (1200,0): dist to O1=1100, dist to O2=300 → effective dist=300
        // HORHE returns the closest (effective dist 100): entityA and entityB.
        var originO2 = TestFixtures.MakeEntity(x: 800, y: 0);
        var entityA = TestFixtures.MakeEntity(x: 200, y: 0);
        var entityB = TestFixtures.MakeEntity(x: 600, y: 0);
        var entityC = TestFixtures.MakeEntity(x: 1200, y: 0);
        var horhe = new HORHE(
            source: new FixedEntitySet(entityA, entityB, entityC),
            origin: new FixedEntitySet(Origin, originO2));
        var context = TestFixtures.MakeContext();

        var result = horhe.Resolve(context);

        result.Entities.Should().HaveCount(2);
        result.Entities.Should().Contain(entityA);
        result.Entities.Should().Contain(entityB);
    }
}
