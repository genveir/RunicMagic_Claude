using RunicMagic.World.Execution;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.SetOperationRunes
{
    // UNION
    public class AN : IEntitySet
    {
        public IEntitySet Left { get; }
        public IEntitySet Right { get; }

        public AN(IEntitySet left, IEntitySet right)
        {
            Left = left;
            Right = right;
        }

        public EntitySet Resolve(SpellContext context)
        {
            var left = Left.Resolve(context);
            var right = Right.Resolve(context);
            var seen = new HashSet<EntityId>();
            var union = new List<Entity>();

            foreach (var entity in left.Entities.Concat(right.Entities))
            {
                if (seen.Add(entity.Id))
                {
                    union.Add(entity);
                }
            }

            var result = new EntitySet(union);
            return result;
        }

        public override string ToString()
        {
            var result = $"AN ( {Left}, {Right} )";
            return result;
        }
    }
}
