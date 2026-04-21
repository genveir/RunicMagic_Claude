using RunicMagic.World.Execution;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.SetOperationRunes
{
    // INTERSECTION
    public class DU : IEntitySet
    {
        public IEntitySet Left { get; }
        public IEntitySet Right { get; }

        public DU(IEntitySet left, IEntitySet right)
        {
            Left = left;
            Right = right;
        }

        public EntitySet Resolve(SpellContext context)
        {
            var left = Left.Resolve(context);
            var right = Right.Resolve(context);
            var rightIds = right.Entities.Select(e => e.Id).ToHashSet();
            var intersection = left.Entities
                .Where(e => rightIds.Contains(e.Id))
                .ToList();
            var result = new EntitySet(intersection);
            return result;
        }

        public override string ToString()
        {
            var result = $"DU ( {Left}, {Right} )";
            return result;
        }
    }
}
