using RunicMagic.World.Execution;

namespace RunicMagic.World.Runes.RuneTypes
{
    public interface IStatement
    {
        void Execute(SpellContext context);
    }
}
