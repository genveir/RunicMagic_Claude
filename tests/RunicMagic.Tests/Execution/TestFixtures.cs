using RunicMagic.World;
using RunicMagic.World.Execution;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Tests.Execution;

internal static class TestFixtures
{
    internal static Entity MakeEntity(int x = 0, int y = 0, int weight = 0)
    {
        var entity = new Entity(new EntityId(Guid.NewGuid()), EntityType.Object, "test");
        entity.X = x;
        entity.Y = y;
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
    private readonly int _value;

    internal FixedNumber(int value)
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

    internal FixedLocation(int x, int y)
    {
        _location = new Location(x, y);
    }

    public Location Evaluate(SpellContext context)
    {
        return _location;
    }
}
