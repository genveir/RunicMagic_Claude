using RunicMagic.World.Execution;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.InvocationRunes
{
    // READ INSCRIPTION
    public class CRIYR : IStatement
    {
        public IEntitySet Target { get; }

        public CRIYR(IEntitySet target)
        {
            Target = target;
        }

        public void Execute(SpellContext context)
        {
            var targets = Target.Resolve(context);
            foreach (var entity in targets.Entities)
            {
                foreach (var inscription in entity.RawInscriptions)
                {
                    context.Result.Add(new InscriptionReadEvent(entity, inscription));
                }
            }
        }

        public override string ToString()
        {
            var result = $"CRIYR ( {Target} )";
            return result;
        }
    }
}
