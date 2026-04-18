using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.EffectRunes
{
    // PUSH
    public class VUN : IStatement
    {
        public IEntitySet ToMove { get; }
        public INumber HowFar { get; }
        public ILocation Origin { get; }

        public VUN(IEntitySet toMove, INumber howFar, ILocation origin)
        {
            ToMove = toMove;
            HowFar = howFar;
            Origin = origin;
        }

        public void Execute(SpellContext context)
        {
            var toMove = ToMove.Resolve(context);
            long distance = HowFar.Evaluate(context).Value;
            var origin = Origin.Evaluate(context);

            long totalWeight = toMove.Entities.Sum(e => e.Weight);
            long executionCost = distance * totalWeight / 1_000_000;

            var drawn = context.DrawPower(executionCost);
            if (drawn < executionCost)
            {
                context.Result.Add(new EffectNotFiredEvent(
                    "VUN",
                    $"Insufficient power: needed {executionCost}, drew {drawn}"
                ));
                return;
            }

            foreach (var entity in toMove.Entities)
            {
                var entityLocation = new Location(entity.X, entity.Y);
                var direction = Direction.FromPoints(origin, entityLocation);
                var destination = entityLocation.Translate(direction, distance);
                entity.X = destination.X;
                entity.Y = destination.Y;
                context.Result.Add(new EntityPushedEvent(entity, distance));
            }
        }

        public override string ToString()
        {
            var result = $"VUN ( {ToMove}, {HowFar}, {Origin} )";
            return result;
        }
    }
}
