using RunicMagic.World.Entities;

namespace RunicMagic.World.Execution;

public class EntitySet
{
    private readonly IReadOnlyList<Entity> entities;

    public EntitySet(IEnumerable<Entity> entities)
    {
        this.entities = entities.ToList();
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

    public override string ToString()
    {
        return $"EntitySet({string.Join(", ", entities.Select(e => e.Label))})";
    }
}