using RunicMagic.Tests.Builders;
using RunicMagic.World;
using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Tests.Execution;

internal static class TestFixtures
{
    internal static SpellContext MakeContext(
        EntitySet? caster = null,
        EntitySet? executor = null,
        WorldModel? world = null,
        SpellResult? result = null)
    {
        return new SpellContext(
            caster ?? new EntitySet([]),
            executor ?? new EntitySet([]),
            world ?? new WorldModel(),
            result ?? new SpellResult()
        );
    }
}

internal class FixedEntitySet : IEntitySet
{
    private readonly EntitySet _resolved;

    internal FixedEntitySet(params Entity[] entities)
    {
        _resolved = new EntitySet(entities);
    }

    public EntitySet Resolve(SpellContext context)
    {
        context.EntityResolutionCount?.UnionWith(_resolved.Entities.Select(e => e.Id));
        return _resolved;
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

internal class FixedLocation : ILocation
{
    private readonly Location _location;

    internal FixedLocation(long x, long y)
    {
        _location = new Location(x, y);
    }

    public Location Evaluate(SpellContext context)
    {
        return _location;
    }
}
