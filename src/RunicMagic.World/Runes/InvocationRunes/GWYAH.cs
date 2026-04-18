using RunicMagic.World.Execution;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.InvocationRunes
{
    // INVOKE
    public class GWYAH : IStatement
    {
        public IEntitySet Target { get; }

        public GWYAH(IEntitySet target)
        {
            Target = target;
        }

        public void Execute(SpellContext context)
        {
            var targets = Target.Resolve(context);
            foreach (var entity in targets.Entities)
            {
                foreach (var inscription in entity.ParsedInscriptions)
                {
                    var forked = context.ForkWithNewExecutor(new EntitySet([entity]));
                    inscription.Execute(forked);
                }
            }
        }

        public override string ToString()
        {
            var result = $"GWYAH ( {Target} )";
            return result;
        }
    }
}
