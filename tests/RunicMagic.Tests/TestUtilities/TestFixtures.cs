using RunicMagic.Controller.Services;
using RunicMagic.Tests.Builders;
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
            world ?? new WorldModel(),
            result ?? new EventTracker()
        );
    }
}

internal class FixedNumber : INumber
{
    private readonly long _value;

    internal FixedNumber(long value)
    {
        _value = value;
    }

    public Number Evaluate(SpellContext context)
    {
        return new Number(_value);
    }
}

internal class FixedPointEntitySet : IEntitySet
{
    private readonly Location _location;

    internal FixedPointEntitySet(long x, long y)
    {
        _location = new Location(x, y);
    }

    public EntitySet Resolve(SpellContext context)
    {
        var entity = new EntityBuilder()
            .WithLocation(_location)
            .WithSize(1, 1)
            .Build();
        return new EntitySet([entity]);
    }
}
