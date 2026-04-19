using RunicMagic.World.Execution;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.NumberRunes
{
    // FOURTEEN SQUARED (196)
    public class DOT : INumber
    {
        public DOT() { }

        public Number Evaluate(SpellContext context)
        {
            var result = new Number(196);
            return result;
        }

        public override string ToString()
        {
            return "DOT";
        }
    }
}
