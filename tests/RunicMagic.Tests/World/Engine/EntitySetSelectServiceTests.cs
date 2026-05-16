using RunicMagic.World;
using RunicMagic.World.Engine;
using RunicMagic.World.Entities;
using RunicMagic.World.Geometry;

namespace RunicMagic.Tests.World.Engine;

public class EntitySetSelectServiceTests
{
    // The service treats everything beyond this radius from the origin (0,0) as a
    // "living world" of fictional entities. Mirrors EntitySetSelectService.FictionalBound.
    private const long FictionalBound = 100_000;

    private static readonly Direction Right = new(1, 0);

    private static (EntitySetSelectService service, WorldModel world) MakeService()
    {
        var world = new WorldModel();
        var service = new EntitySetSelectService(world, new RayCastService(world));
        return (service, world);
    }

    private static Entity EntityAt(long x, long y, long width = 100, long height = 100)
    {
        return new EntityBuilder().WithLocation(x, y).WithSize(width, height).Build();
    }

    private static Entity WithScope(EntitySetSelectionResult scope)
    {
        return new EntityBuilder().WithScope(() => scope).Build();
    }

    // ── GetAll ────────────────────────────────────────────────────────────────

    [Fact]
    public void GetAll_ReturnsEveryWorldEntity_WithFullFictionalCount()
    {
        var (service, world) = MakeService();
        var a = EntityAt(0, 0);
        var b = EntityAt(100, 0);
        world.Add(a);
        world.Add(b);

        var result = service.GetAll();

        result.Entities.Should().BeEquivalentTo([a, b]);
        result.FictionalResults.Should().Be(1_000_000_000);
    }

    // ── GetAllInRangeFrom ─────────────────────────────────────────────────────

    [Fact]
    public void GetAllInRangeFrom_EmptyOriginSet_ReturnsEmptyWithNoFictionalResults()
    {
        var (service, _) = MakeService();

        var result = service.GetAllInRangeFrom([], range: 1000);

        result.Entities.Should().BeEmpty();
        result.FictionalResults.Should().Be(0);
    }

    [Fact]
    public void GetAllInRangeFrom_EntityWithinRange_IsReturned()
    {
        var (service, world) = MakeService();
        var origin = EntityAt(0, 0);
        var nearby = EntityAt(150, 0);
        world.Add(origin);
        world.Add(nearby);

        var result = service.GetAllInRangeFrom([origin], range: 1000);

        result.Entities.Should().Contain(nearby);
    }

    [Fact]
    public void GetAllInRangeFrom_QueryCircleWhollyInsideFictionalBound_ReportsNoFictionalResults()
    {
        // Origin at (0,0), radius 1000: the whole query circle sits inside the
        // fictional bound, so nothing is fictional.
        var (service, world) = MakeService();
        var origin = EntityAt(0, 0);
        world.Add(origin);

        var result = service.GetAllInRangeFrom([origin], range: 1000);

        result.FictionalResults.Should().Be(0);
    }

    [Fact]
    public void GetAllInRangeFrom_QueryCircleWhollyOutsideFictionalBound_FictionalResultsCoverWholeArea()
    {
        // Origin far outside the fictional bound: the entire query circle is
        // fictional. area = pi * r^2 = pi * 1000^2 ~= 3,141,592 ; /1_000_000 -> 3
        var (service, world) = MakeService();
        var origin = EntityAt(FictionalBound * 10, 0);
        world.Add(origin);

        var result = service.GetAllInRangeFrom([origin], range: 1000);

        result.Entities.Should().BeEmpty();
        result.FictionalResults.Should().Be(3);
    }

    // ── GetAllInRay ───────────────────────────────────────────────────────────

    [Fact]
    public void GetAllInRay_ReturnsEntitiesAlongRay()
    {
        var (service, world) = MakeService();
        var wall = EntityAt(500, 0);
        world.Add(wall);

        var result = service.GetAllInRay(new Location(0, 0), Right, range: 50_000);

        result.Entities.Should().Contain(wall);
    }

    [Fact]
    public void GetAllInRay_RayStartsOutsideAndMissesFictionalBound_WholeRangeIsFictional()
    {
        // from (0, 1_000_000) pointing +X never enters the fictional-bound circle,
        // so the full range is fictional. range / 10_000 = 50_000 / 10_000 = 5
        var (service, _) = MakeService();

        var result = service.GetAllInRay(new Location(0, 1_000_000), Right, range: 50_000);

        result.Entities.Should().BeEmpty();
        result.FictionalResults.Should().Be(5);
    }

    // ── GetFirstInRay ─────────────────────────────────────────────────────────

    [Fact]
    public void GetFirstInRay_ReturnsClosestNonSourceEntity()
    {
        var (service, world) = MakeService();
        var caster = EntityAt(0, 0);
        var near = EntityAt(500, 0);
        var far = EntityAt(1000, 0);
        world.Add(caster);
        world.Add(near);
        world.Add(far);

        var result = service.GetFirstInRay(
            filter: [caster],
            from: caster.Location,
            direction: Right,
            range: 50_000,
            filterTranslucent: true);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(near);
    }

    [Fact]
    public void GetFirstInRay_OnlyTranslucentOnRay_FilteredOut_ReturnsEmpty()
    {
        var (service, world) = MakeService();
        var caster = EntityAt(0, 0);
        var glass = new EntityBuilder().WithLocation(500, 0).WithSize(100, 100).WithTranslucency().Build();
        world.Add(caster);
        world.Add(glass);

        var result = service.GetFirstInRay(
            filter: [caster],
            from: caster.Location,
            direction: Right,
            range: 50_000,
            filterTranslucent: true);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void GetInAllInRayExceptSourceEntities_ExcludesFilteredEntities()
    {
        var (service, world) = MakeService();
        var caster = EntityAt(0, 0);
        var wall = EntityAt(500, 0);
        world.Add(caster);
        world.Add(wall);

        var result = service.GetInAllInRayExceptSourceEntities(
            filter: [caster],
            from: caster.Location,
            direction: Right,
            range: 50_000);

        result.Entities.Should().NotContain(caster);
        result.Entities.Should().Contain(wall);
    }

    // ── GetUnionScope ─────────────────────────────────────────────────────────

    [Fact]
    public void GetUnionScope_EmptyInput_ReturnsEmptyWithNoFictionalResults()
    {
        var (service, _) = MakeService();

        var result = service.GetUnionScope([]);

        result.Entities.Should().BeEmpty();
        result.FictionalResults.Should().Be(0);
    }

    [Fact]
    public void GetUnionScope_DeduplicatesAndTakesMaxFictionalResults()
    {
        var (service, _) = MakeService();
        var a = EntityAt(0, 0);
        var b = EntityAt(1, 0);
        var c = EntityAt(2, 0);
        var first = WithScope(new EntitySetSelectionResult(Entities: [a, b], FictionalResults: 5));
        var second = WithScope(new EntitySetSelectionResult(Entities: [b, c], FictionalResults: 2));

        var result = service.GetUnionScope([first, second]);

        result.Entities.Should().BeEquivalentTo([a, b, c]);
        result.FictionalResults.Should().Be(5);
    }

    [Fact]
    public void GetUnionScope_EntityWithoutScope_ContributesNothing()
    {
        var (service, _) = MakeService();
        var withoutScope = EntityAt(0, 0);

        var result = service.GetUnionScope([withoutScope]);

        result.Entities.Should().BeEmpty();
        result.FictionalResults.Should().Be(0);
    }

    // ── GetIntersectScope ─────────────────────────────────────────────────────

    [Fact]
    public void GetIntersectScope_EmptyInput_ReturnsEmptyWithNoFictionalResults()
    {
        var (service, _) = MakeService();

        var result = service.GetIntersectScope([]);

        result.Entities.Should().BeEmpty();
        result.FictionalResults.Should().Be(0);
    }

    [Fact]
    public void GetIntersectScope_ReturnsSharedMembersAndMinFictionalResults()
    {
        var (service, _) = MakeService();
        var a = EntityAt(0, 0);
        var b = EntityAt(1, 0);
        var c = EntityAt(2, 0);
        var first = WithScope(new EntitySetSelectionResult(Entities: [a, b], FictionalResults: 7));
        var second = WithScope(new EntitySetSelectionResult(Entities: [b, c], FictionalResults: 4));

        var result = service.GetIntersectScope([first, second]);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(b);
        result.FictionalResults.Should().Be(4);
    }

    [Fact]
    public void GetIntersectScope_EntityWithoutScope_MakesIntersectionEmpty()
    {
        var (service, _) = MakeService();
        var shared = EntityAt(0, 0);
        var withScope = WithScope(new EntitySetSelectionResult(Entities: [shared], FictionalResults: 9));
        var withoutScope = EntityAt(1, 0);

        var result = service.GetIntersectScope([withScope, withoutScope]);

        result.Entities.Should().BeEmpty();
    }
}
