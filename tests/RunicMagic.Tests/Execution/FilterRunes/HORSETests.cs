using FluentAssertions;
using RunicMagic.World;
using RunicMagic.World.Runes.FilterRunes;
using Xunit;

namespace RunicMagic.Tests.Execution.FilterRunes;

public class HORSETests
{
    private static Entity MakeEntity(long x, long y)
    {
        return TestFixtures.MakeEntity(x: x, y: y);
    }

    [Fact]
    public void Resolve_SingleEntity_IsReturned()
    {
        var entity = MakeEntity(x: 500, y: 0);
        var horse = new HORSE(source: new FixedEntitySet(entity), origin: new FixedLocation(0, 0));
        var context = TestFixtures.MakeContext();

        var result = horse.Resolve(context);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(entity);
    }

    [Fact]
    public void Resolve_MultipleEntities_ReturnsOnlyFarthest()
    {
        // nearest edges: near=450, mid=950, far=1950
        var near = MakeEntity(x: 500, y: 0);
        var mid = MakeEntity(x: 1000, y: 0);
        var far = MakeEntity(x: 2000, y: 0);
        var horse = new HORSE(source: new FixedEntitySet(near, mid, far), origin: new FixedLocation(0, 0));
        var context = TestFixtures.MakeContext();

        var result = horse.Resolve(context);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(far);
    }

    [Fact]
    public void Resolve_TiedFarthest_ReturnsAllTied()
    {
        var near = MakeEntity(x: 500, y: 0);
        var farA = MakeEntity(x: 2000, y: 0);
        var farB = MakeEntity(x: 2000, y: 0);
        var horse = new HORSE(source: new FixedEntitySet(near, farA, farB), origin: new FixedLocation(0, 0));
        var context = TestFixtures.MakeContext();

        var result = horse.Resolve(context);

        result.Entities.Should().HaveCount(2);
        result.Entities.Should().Contain(farA);
        result.Entities.Should().Contain(farB);
    }

    [Fact]
    public void Resolve_UsesOriginFromLocation()
    {
        // From origin (900, 0): farA nearest edge at 50, farB nearest edge at 550
        var farA = MakeEntity(x: 1000, y: 0);
        var farB = MakeEntity(x: 1500, y: 0);
        var horse = new HORSE(source: new FixedEntitySet(farA, farB), origin: new FixedLocation(900, 0));
        var context = TestFixtures.MakeContext();

        var result = horse.Resolve(context);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(farB);
    }

    [Fact]
    public void Resolve_EmptySource_ReturnsEmpty()
    {
        var horse = new HORSE(source: new FixedEntitySet(), origin: new FixedLocation(0, 0));
        var context = TestFixtures.MakeContext();

        var result = horse.Resolve(context);

        result.Entities.Should().BeEmpty();
    }
}
