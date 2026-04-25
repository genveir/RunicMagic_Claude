namespace RunicMagic.World.Execution;

public class EntitySet
{
    private readonly IReadOnlyList<Entity> _entities;

    public EntitySet(IReadOnlyList<Entity> entities)
    {
        _entities = entities;
    }

    public IReadOnlyList<Entity> Entities
    {
        get
        {
            return _entities;
        }
    }

    public EntitySet GetScope()
    {
        var seen = new HashSet<EntityId>();
        var scopeEntities = new List<Entity>();
        foreach (var entity in _entities)
        {
            var scope = entity.Scope?.Invoke() ?? [];
            foreach (var member in scope)
            {
                if (seen.Add(member.Id))
                {
                    scopeEntities.Add(member);
                }
            }
        }
        return new EntitySet(scopeEntities);
    }

    public override string ToString()
    {
        return $"EntitySet({string.Join(", ", _entities.Select(e => e.Label))})";
    }
}