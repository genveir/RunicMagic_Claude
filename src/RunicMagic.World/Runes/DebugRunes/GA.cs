using RunicMagic.World.Execution;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.DebugRunes
{
    public class GA : IEntitySet
    {
        public EntitySet Resolve(SpellContext context)
        {
            if (context.EntityResolutionCount != null)
            {
                context.DrawPower(1000000000);
                context.Result.Add(new DebugOutputEvent("Accessing the global scope burned out your fragile mortal soul."));
            }

            return new EntitySet(context.World.GetAll());
        }
    }
}
