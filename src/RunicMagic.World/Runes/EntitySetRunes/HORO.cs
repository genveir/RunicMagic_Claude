using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.EntitySetRunes
{
    // NEAR — all entities within howFar mm of any entity in origin set (nearest bounding edge to nearest bounding edge)
    public class HORO : IEntitySet
    {
        public INumber HowFar { get; }
        public IEntitySet Origin { get; }

        public HORO(INumber howFar, IEntitySet origin)
        {
            HowFar = howFar;
            Origin = origin;
        }

        public EntitySet Resolve(SpellContext context)
        {
            var radius = HowFar.Evaluate(context);
            var originSet = Origin.Resolve(context);
            var originRects = originSet.Entities.Select(e => Bounds(e)).ToList();
            var entities = context.World.GetAll()
                .Where(e => e.GetDistanceFromSet(originRects) <= (double)radius.Value)
                .ToList();
            var result = new EntitySet(entities);
            context.EntityResolutionCount?.UnionWith(result.Entities.Select(e => e.Id));
            return result;
        }

        private static Rectangle Bounds(Entity e)
        {
            var bounds = new Rectangle(e.Location, e.Width, e.Height, e.Angle);
            return bounds;
        }

        public override string ToString()
        {
            var result = $"HORO ( {HowFar}, {Origin} )";
            return result;
        }
    }
}
