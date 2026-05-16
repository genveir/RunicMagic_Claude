using RunicMagic.World.Execution;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.EntitySetRunes;

public class PA : IEntitySet
{
    public IEntitySet ToGetScopeOf { get; }

    public PA(IEntitySet toGetScopeOf)
    {
        ToGetScopeOf = toGetScopeOf;
    }

    public EntitySet Resolve(SpellContext context)
    {
        var inputSet = ToGetScopeOf.Resolve(context);

        var entitySelection = context.EngineAPI.EntitySetSelectService
            .GetIntersectScope(inputSet.Entities);

        context.EntityResolutionCount?.UnionWith(entitySelection);

        var result = new EntitySet(entitySelection.Entities);
        return result;
    }

    public override string ToString()
    {
        var result = $"PA ( {ToGetScopeOf} )";
        return result;
    }
}
