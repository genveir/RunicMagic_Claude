using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.DebugRunes
{
    public class DETAILS : IStatement
    {
        private readonly IEntitySet entitySet;

        public DETAILS(IEntitySet entitySet)
        {
            this.entitySet = entitySet;
        }

        public void Execute(SpellContext context)
        {
            var entities = entitySet.Resolve(context);

            var casterEntities = context.Caster.Entities;
            if (casterEntities.Count == 0)
            {
                context.Result.Add(new DebugOutputEvent("DETAILS: No caster entity found."));
                return;
            }
            var caster = casterEntities[0];

            context.Result.Add(new DebugOutputEvent($"DETAILS: Caster is {caster.Label} at location {caster.Location}."));

            if (entities?.Entities == null)
            {
                context.Result.Add(new DebugOutputEvent("DETAILS: No entities found."));
            }
            else
            {
                foreach (var entity in entities.Entities)
                {
                    var distance = entity.GetDistance(context.Caster.Entities[0].Location);
                    context.Result.Add(new DebugOutputEvent($"DETAILS: {entity.Label} is at location {entity.Location} with width {entity.Width} and height {entity.Height} at rotation {entity.Angle}."));
                    context.Result.Add(new DebugOutputEvent($"DETAILS: {entity.Label} is {distance}mm away."));
                }
            }

            if (!caster.PointingDirection.HasValue)
            {
                return;
            }

            context.Result.Add(new DebugOutputEvent($"DETAILS: Caster is pointing at {caster.PointingDirection.Value.X}, {caster.PointingDirection.Value.Y}."));

            var rayCast = new RayCastService(context.World);
            var castResult = rayCast.Cast(caster.Id, caster.Location, caster.PointingDirection.Value);

            if (castResult.HitEntity == null)
            {
                context.Result.Add(new DebugOutputEvent("DETAILS: Ray cast hit nothing."));
            }
            else
            {
                var hitEntity = castResult.HitEntity;
                context.Result.Add(new DebugOutputEvent($"DETAILS: Ray cast hit {hitEntity.Label} at {castResult.LocationOfIntersect}."));
            }

        }
    }
}
