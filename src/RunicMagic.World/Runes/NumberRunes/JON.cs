using RunicMagic.World.Execution;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.NumberRunes
{
    // ZERO
    public class JON : INumber
    {
        public JON() { }

        public Number Evaluate(SpellContext context)
        {
            var result = new Number(0);
            return result;
        }

        public override string ToString()
        {
            return "JON";
        }
    }
}
