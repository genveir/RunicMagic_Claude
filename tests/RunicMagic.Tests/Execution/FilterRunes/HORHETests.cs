using FluentAssertions;
using RunicMagic.World;
using RunicMagic.World.Runes.FilterRunes;
using Xunit;

namespace RunicMagic.Tests.Execution.FilterRunes;

public class HORHETests
{
    private static Entity MakeEntity(long x, long y)
    {
        return TestFixtures.MakeEntity(x: x, y: y);
    }

    [Fact]
    public void Resolve_SingleEntity_IsReturned()
    {
        var entity = MakeEntity(x: 500, y: 0);
        var horhe = new HORHE(source: new FixedEntitySet(entity), origin: new FixedLocation(0, 0));
        var context = TestFixtures.MakeContext();

        var result = horhe.Resolve(context);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(entity);
    }

    [Fact]
    public void Resolve_MultipleEntities_ReturnsOnlyClosest()
    {
        // nearest edges: near=450, mid=950, far=1950
        var near = MakeEntity(x: 500, y: 0);
        var mid = MakeEntity(x: 1000, y: 0);
        var far = MakeEntity(x: 2000, y: 0);
        var horhe = new HORHE(source: new FixedEntitySet(near, mid, far), origin: new FixedLocation(0, 0));
        var context = TestFixtures.MakeContext();

        var result = horhe.Resolve(context);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(near);
    }

    [Fact]
    public void Resolve_TiedClosest_ReturnsAllTied()
    {
        // Both nearA and nearB have their nearest edge at x=450
        var nearA = MakeEntity(x: 500, y: 0);
        var nearB = MakeEntity(x: 500, y: 0);
        var far = MakeEntity(x: 2000, y: 0);
        var horhe = new HORHE(source: new FixedEntitySet(nearA, nearB, far), origin: new FixedLocation(0, 0));
        var context = TestFixtures.MakeContext();

        var result = horhe.Resolve(context);

        result.Entities.Should().HaveCount(2);
        result.Entities.Should().Contain(nearA);
        result.Entities.Should().Contain(nearB);
    }

    [Fact]
    public void Resolve_UsesOriginFromLocation()
    {
        // From origin (900, 0): nearA nearest edge at 50, nearB nearest edge at 550
        var nearA = MakeEntity(x: 1000, y: 0);
        var nearB = MakeEntity(x: 1500, y: 0);
        var horhe = new HORHE(source: new FixedEntitySet(nearA, nearB), origin: new FixedLocation(900, 0));
        var context = TestFixtures.MakeContext();

        var result = horhe.Resolve(context);

        result.Entities.Should().ContainSingle().Which.Should().BeSameAs(nearA);
    }

    [Fact]
    public void Resolve_EmptySource_ReturnsEmpty()
    {
        var horhe = new HORHE(source: new FixedEntitySet(), origin: new FixedLocation(0, 0));
        var context = TestFixtures.MakeContext();

        var result = horhe.Resolve(context);

        result.Entities.Should().BeEmpty();
    }
}
