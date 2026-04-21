using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.FilterRunes
{
    public class HORSE : IEntitySet
    {
        public IEntitySet Source { get; }
        public IEntitySet Origin { get; }

        public HORSE(IEntitySet source, IEntitySet origin)
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
            var maxDistance = source.Entities.Max(e => e.GetDistanceFromSet(originRects));
            var farthest = source.Entities
                .Where(e => e.GetDistanceFromSet(originRects) == maxDistance)
                .ToList();
            var result = new EntitySet(farthest);
            return result;
        }

        private static Rectangle Bounds(Entity e)
        {
            var bounds = new Rectangle(e.Location, e.Width, e.Height, e.Angle);
            return bounds;
        }

        public override string ToString()
        {
            var result = $"HORSE ( {Source}, {Origin} )";
            return result;
        }
    }
}
