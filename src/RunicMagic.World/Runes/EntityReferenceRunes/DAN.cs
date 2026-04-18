using RunicMagic.World.Execution;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.EntityReferenceRunes
{
    // POINTING AT (aimed entity via ray cast)
    public class DAN : IEntitySet
    {
        public DAN() { }

        public EntitySet Resolve(SpellContext context)
        {
            var casterEntities = context.Caster.Entities;
            if (casterEntities.Count == 0)
            {
                return new EntitySet([]);
            }

            var caster = casterEntities[0];
            if (!caster.PointingDirection.HasValue)
            {
                return new EntitySet([]);
            }

            var rayCast = new RayCastService(context.World);
            var castResult = rayCast.Cast(caster.Id, caster.X, caster.Y, caster.PointingDirection.Value);

            if (castResult.HitEntity == null)
            {
                return new EntitySet([]);
            }

            var result = new EntitySet([castResult.HitEntity]);
            return result;
        }

        public override string ToString()
        {
            return "DAN";
        }
    }
}
