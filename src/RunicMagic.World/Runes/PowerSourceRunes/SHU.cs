using RunicMagic.World.Execution;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.PowerSourceRunes
{
    // WITH POWER SOURCE
    public class SHU : IStatement
    {
        public IEntitySet Source { get; }
        public IStatement Statement { get; }

        public SHU(IEntitySet source, IStatement statement)
        {
            Source = source;
            Statement = statement;
        }

        public void Execute(SpellContext context)
        {
            var source = Source.Resolve(context);
            context.PushPowerSource(source);
            Statement.Execute(context);
            context.PopPowerSource();
        }

        public override string ToString()
        {
            var result = $"SHU ( {Source}, {Statement} )";
            return result;
        }
    }
}
