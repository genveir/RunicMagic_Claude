using RunicMagic.World;
using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Tests.Execution;

internal static class TestFixtures
{
    internal static Entity MakeEntity(long x = 0, long y = 0, long weight = 0)
    {
        var entity = new Entity(new EntityId(Guid.NewGuid()), EntityType.Object, "test");
        entity.Location = new Location(x, y);
        entity.Width = 100;
        entity.Height = 100;
        entity.Weight = weight;
        return entity;
    }

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
