using RunicMagic.World.Execution;
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
            var distance = HowFar.Evaluate(context).Value;
            var origin = Origin.Evaluate(context);

            var totalWeight = toMove.Entities.Sum(e => e.Weight);
            var executionCost = distance * totalWeight / 1_000_000;

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
                var dx = (double)(entity.X - origin.X);
                var dy = (double)(entity.Y - origin.Y);
                var length = Math.Sqrt(dx * dx + dy * dy);

                double dirX, dirY;
                if (length == 0.0)
                {
                    var angle = Random.Shared.NextDouble() * 2 * Math.PI;
                    dirX = Math.Cos(angle);
                    dirY = Math.Sin(angle);
                }
                else
                {
                    dirX = dx / length;
                    dirY = dy / length;
                }

                entity.X += (int)Math.Round(dirX * distance);
                entity.Y += (int)Math.Round(dirY * distance);
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
