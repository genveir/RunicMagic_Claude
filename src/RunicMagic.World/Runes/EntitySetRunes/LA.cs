using RunicMagic.World.Execution;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.EntitySetRunes;

// SCOPE OF
public class LA : IEntitySet
{
    public IEntitySet ToGetScopeOf { get; }

    public LA(IEntitySet toGetScopeOf)
    {
        ToGetScopeOf = toGetScopeOf;
    }

    public EntitySet Resolve(SpellContext context)
    {
        var inputSet = ToGetScopeOf.Resolve(context);

        var entitySelection = context.EngineAPI.EntitySetSelectService
            .GetUnionScope(inputSet.Entities);

        context.EntityResolutionCount?.UnionWith(entitySelection);

        var result = new EntitySet(entitySelection.Entities);
        return result;
    }

    public override string ToString()
    {
        var result = $"LA ( {ToGetScopeOf} )";
        return result;
    }
}
