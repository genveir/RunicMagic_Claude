using FluentAssertions;
using RunicMagic.World;
using RunicMagic.World.Runes.FilterRunes;
using Xunit;

namespace RunicMagic.Tests.Execution.FilterRunes;

public class HORILTests
{
    private static Entity MakeEntity(long x, long y)
    {
        return TestFixtures.MakeEntity(x: x, y: y);
    }

    [Fact]
    public void Resolve_EntityWithDistanceWithinRange_IsReturned()
    {
        // Entity centre at (500, 0), width 100 → nearest edge at x=450, distance 450 from origin
        var entity = MakeEntity(x: 500, y: 0);
        var horil = new HORIL(
            source: new FixedEntitySet(entity),
            lower: new FixedNumber(100),
            upper: new FixedNumber(1000),
            origin: new FixedLocation(0, 0));
        var context = TestFixtures.MakeContext();

        var result = horil.Resolve(context);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(entity);
    }

    [Fact]
    public void Resolve_EntityWithDistanceBelowLower_IsNotReturned()
    {
        // Entity centre at (50, 0), width 100 → entity contains origin, distance 0
        var entity = MakeEntity(x: 50, y: 0);
        var horil = new HORIL(
            source: new FixedEntitySet(entity),
            lower: new FixedNumber(100),
            upper: new FixedNumber(1000),
            origin: new FixedLocation(0, 0));
        var context = TestFixtures.MakeContext();

        var result = horil.Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_EntityWithDistanceAboveUpper_IsNotReturned()
    {
        // Entity centre at (2000, 0), width 100 → nearest edge at x=1950, distance 1950 from origin
        var entity = MakeEntity(x: 2000, y: 0);
        var horil = new HORIL(
            source: new FixedEntitySet(entity),
            lower: new FixedNumber(100),
            upper: new FixedNumber(1000),
            origin: new FixedLocation(0, 0));
        var context = TestFixtures.MakeContext();

        var result = horil.Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_EntityWithDistanceEqualToLower_IsNotReturned()
    {
        // Entity centre at (150, 0), width 100 → nearest edge at x=100, distance exactly 100
        var entity = MakeEntity(x: 150, y: 0);
        var horil = new HORIL(
            source: new FixedEntitySet(entity),
            lower: new FixedNumber(100),
            upper: new FixedNumber(1000),
            origin: new FixedLocation(0, 0));
        var context = TestFixtures.MakeContext();

        var result = horil.Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_EntityWithDistanceEqualToUpper_IsNotReturned()
    {
        // Entity centre at (1050, 0), width 100 → nearest edge at x=1000, distance exactly 1000
        var entity = MakeEntity(x: 1050, y: 0);
        var horil = new HORIL(
            source: new FixedEntitySet(entity),
            lower: new FixedNumber(100),
            upper: new FixedNumber(1000),
            origin: new FixedLocation(0, 0));
        var context = TestFixtures.MakeContext();

        var result = horil.Resolve(context);

        result.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_MultipleEntities_ReturnsOnlyThoseInRange()
    {
        // near: inside origin (distance 0), mid: nearest edge at 450, far: nearest edge at 1950
        var near = MakeEntity(x: 50, y: 0);
        var mid = MakeEntity(x: 500, y: 0);
        var far = MakeEntity(x: 2000, y: 0);
        var horil = new HORIL(
            source: new FixedEntitySet(near, mid, far),
            lower: new FixedNumber(100),
            upper: new FixedNumber(1000),
            origin: new FixedLocation(0, 0));
        var context = TestFixtures.MakeContext();

        var result = horil.Resolve(context);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(mid);
    }

    [Fact]
    public void Resolve_UsesOriginFromLocation()
    {
        // Entity at (1000, 0); origin at (900, 0) → nearest edge at (950, 0), distance 50
        var entity = MakeEntity(x: 1000, y: 0);
        var horil = new HORIL(
            source: new FixedEntitySet(entity),
            lower: new FixedNumber(10),
            upper: new FixedNumber(100),
            origin: new FixedLocation(900, 0));
        var context = TestFixtures.MakeContext();

        var result = horil.Resolve(context);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(entity);
    }

    [Fact]
    public void Resolve_EmptySource_ReturnsEmpty()
    {
        var horil = new HORIL(
            source: new FixedEntitySet(),
            lower: new FixedNumber(100),
            upper: new FixedNumber(1000),
            origin: new FixedLocation(0, 0));
        var context = TestFixtures.MakeContext();

        var result = horil.Resolve(context);

        result.Entities.Should().BeEmpty();
    }
}
