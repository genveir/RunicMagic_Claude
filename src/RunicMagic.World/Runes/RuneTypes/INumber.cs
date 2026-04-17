using RunicMagic.World.Execution;

namespace RunicMagic.World.Runes.RuneTypes
{
    public interface INumber
    {
        Number Evaluate(SpellContext context);
    }
}
