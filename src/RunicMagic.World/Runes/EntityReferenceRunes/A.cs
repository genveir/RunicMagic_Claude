using RunicMagic.World.Execution;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.EntityReferenceRunes
{
    // ME (caster)
    public class A : IEntitySet
    {
        public A() { }

        public EntitySet Resolve(SpellContext context)
        {
            var result = context.Caster;
            context.EntityResolutionCount?.UnionWith(result.Entities.Select(e => e.Id));
            return result;
        }

        public override string ToString()
        {
            return "A";
        }
    }
}
