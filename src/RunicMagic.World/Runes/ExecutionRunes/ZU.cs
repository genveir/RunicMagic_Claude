using RunicMagic.World.Execution;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.ExecutionRunes
{
    // EXECUTE
    public class ZU : IExecutableStatement
    {
        public IStatement Statement { get; }

        public ZU(IStatement statement)
        {
            Statement = statement;
        }

        public void Execute(SpellContext context)
        {
            Statement.Execute(context);
        }

        public override string ToString()
        {
            var result = $"ZU ( {Statement} )";
            return result;
        }
    }
}
