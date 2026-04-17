using RunicMagic.World.Execution;

namespace RunicMagic.World.Runes.RuneTypes
{
    public interface IExecutableStatement
    {
        void Execute(SpellContext context);
    }
}
