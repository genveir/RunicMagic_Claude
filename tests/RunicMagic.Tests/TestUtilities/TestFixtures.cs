using RunicMagic.Controller.Services;
using RunicMagic.World;
using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Tests.TestUtilities;

internal static class TestFixtures
{
    internal static SpellContext MakeContext(
        EntitySet? caster = null,
        EntitySet? executor = null,
        WorldModel? world = null,
        EventTracker? result = null)
    {
        return new SpellContext(
            caster ?? new EntitySet([]),
            executor ?? new EntitySet([]),
            world ?? new WorldModelBuilder().Build(),
            result ?? new EventTracker()
        );
    }
}

internal class FixedNumber : INumber
{
    private readonly long value;

    internal FixedNumber(long value)
    {
        this.value = value;
    }

    public Number Evaluate(SpellContext context)
    {
        return new Number(value);
    }
}

internal class FixedPointEntitySet : IEntitySet
{
    private readonly Location location;

    internal FixedPointEntitySet(long x, long y)
    {
        location = new Location(x, y);
    }

    public EntitySet Resolve(SpellContext context)
    {
        var entity = new EntityBuilder()
            .WithLocation(location)
            .WithSize(1, 1)
            .Build();
        return new EntitySet([entity]);
    }
}
