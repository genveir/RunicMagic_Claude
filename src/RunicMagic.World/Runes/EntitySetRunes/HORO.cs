using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.EntitySetRunes
{
    // NEAR — all entities within howFar mm of origin (nearest bounding edge)
    public class HORO : IEntitySet
    {
        public INumber HowFar { get; }
        public ILocation Origin { get; }

        public HORO(INumber howFar, ILocation origin)
        {
            HowFar = howFar;
            Origin = origin;
        }

        public EntitySet Resolve(SpellContext context)
        {
            var radius = HowFar.Evaluate(context);
            var origin = Origin.Evaluate(context);
            var entities = context.World.GetAll()
                .WithinDistance(origin, (double)radius.Value)
                .ToList();
            var result = new EntitySet(entities);
            context.EntityResolutionCount?.UnionWith(result.Entities.Select(e => e.Id));
            return result;
        }

        public override string ToString()
        {
            var result = $"HORO ( {HowFar}, {Origin} )";
            return result;
        }
    }
}
