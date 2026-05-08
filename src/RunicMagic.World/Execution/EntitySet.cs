using RunicMagic.World.Entities;

namespace RunicMagic.World.Execution;

public class EntitySet
{
    private readonly IReadOnlyList<Entity> entities;

    public EntitySet(IReadOnlyList<Entity> entities)
    {
        this.entities = entities;
    }

    public IReadOnlyList<Entity> Entities
    {
        get
        {
            return entities
                .Where(e => e.StructuralIntegrity.CurrentIntegrity > 0)
                .ToList();
        }
    }

    public EntitySet GetScope()
    {
        var seen = new HashSet<EntityId>();
        var scopeEntities = new List<Entity>();
        foreach (var entity in entities)
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
        return $"EntitySet({string.Join(", ", entities.Select(e => e.Label))})";
    }
}