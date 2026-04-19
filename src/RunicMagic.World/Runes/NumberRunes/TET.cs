using RunicMagic.World.Execution;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.NumberRunes
{
    // THREE
    public class TET : INumber
    {
        public TET() { }

        public Number Evaluate(SpellContext context)
        {
            var result = new Number(3);
            return result;
        }

        public override string ToString()
        {
            return "TET";
        }
    }
}
