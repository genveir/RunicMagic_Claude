using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.EntityReferenceRunes
{
    // INDICATING (consciously indicated entity)
    public class KAL : IEntitySet
    {
        public KAL() { }

        public EntitySet Resolve(SpellContext context)
        {
            var casterEntities = context.Caster.Entities;
            if (casterEntities.Count == 0)
            {
                return new EntitySet([]);
            }

            var caster = casterEntities[0];
            if (caster.IndicateTarget == null)
            {
                return new EntitySet([]);
            }

            var target = context.World.Find(caster.IndicateTarget.EntityId);
            if (target == null)
            {
                return new EntitySet([]);
            }

            // Self-indicate: no raycast needed
            if (caster.IndicateTarget.Direction == null)
            {
                var selfResult = new EntitySet([target]);
                context.EntityResolutionCount?.Add(target.Id);
                return selfResult;
            }

            var rayCast = new RayCastService(context.World);
            var castResult = rayCast.Cast(caster.Id, caster.Location, caster.IndicateTarget.Direction.Value, skipTranslucent: false);

            if (castResult.HitEntity?.Id != caster.IndicateTarget.EntityId)
            {
                return new EntitySet([]);
            }

            var distance = castResult.LocationOfIntersect.GetDistanceTo(caster.Location);
            if (distance > 1000)
            {
                return new EntitySet([]);
            }

            var result = new EntitySet([target]);
            context.EntityResolutionCount?.Add(target.Id);
            return result;
        }

        public override string ToString()
        {
            return "KAL";
        }
    }
}
