using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.FilterRunes
{
    public class HORHE : IEntitySet
    {
        public IEntitySet Source { get; }
        public IEntitySet Origin { get; }

        public HORHE(IEntitySet source, IEntitySet origin)
        {
            Source = source;
            Origin = origin;
        }

        public EntitySet Resolve(SpellContext context)
        {
            var source = Source.Resolve(context);
            if (!source.Entities.Any())
            {
                return new EntitySet([]);
            }
            var originSet = Origin.Resolve(context);
            var originRects = originSet.Entities.Select(e => Bounds(e)).ToList();
            var minDistance = source.Entities.Min(e => e.GetDistanceFromSet(originRects));
            var closest = source.Entities
                .Where(e => e.GetDistanceFromSet(originRects) == minDistance)
                .ToList();
            var result = new EntitySet(closest);
            return result;
        }

        private static Rectangle Bounds(Entity e)
        {
            var bounds = new Rectangle(e.Location, e.Width, e.Height, e.Angle);
            return bounds;
        }

        public override string ToString()
        {
            var result = $"HORHE ( {Source}, {Origin} )";
            return result;
        }
    }
}
