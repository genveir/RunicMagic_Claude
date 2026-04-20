using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.EffectRunes
{
    // PULL
    public class VAR : IStatement
    {
        public IEntitySet ToMove { get; }
        public INumber HowFar { get; }
        public ILocation Origin { get; }

        public VAR(IEntitySet toMove, INumber howFar, ILocation origin)
        {
            ToMove = toMove;
            HowFar = howFar;
            Origin = origin;
        }

        public void Execute(SpellContext context)
        {
            var toMove = ToMove.Resolve(context);
            var distance = HowFar.Evaluate(context).Value;
            var origin = Origin.Evaluate(context);

            var totalWeight = toMove.Entities.Sum(e => e.Weight);
            var executionCost = distance * totalWeight / 1_000_000;

            var drawn = context.DrawPower(executionCost);
            if (drawn < executionCost)
            {
                context.Result.Add(new EffectNotFiredEvent(
                    "VAR",
                    $"Insufficient power: needed {executionCost}, drew {drawn}"
                ));
                return;
            }

            foreach (var entity in toMove.Entities)
            {
                var direction = Direction.FromPoints(entity.Location, origin);
                var destination = entity.Location.Translate(direction, distance);
                entity.Location = destination;

                if (distance > 0)
                {
                    context.Result.Add(new EntityPulledEvent(entity, distance));
                }
            }
        }

        public override string ToString()
        {
            var result = $"VAR ( {ToMove}, {HowFar}, {Origin} )";
            return result;
        }
    }
}
