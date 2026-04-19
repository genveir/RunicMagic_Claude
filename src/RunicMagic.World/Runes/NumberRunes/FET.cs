using RunicMagic.World.Execution;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.NumberRunes
{
    // FIVE
    public class FET : INumber
    {
        public FET() { }

        public Number Evaluate(SpellContext context)
        {
            var result = new Number(5);
            return result;
        }

        public override string ToString()
        {
            return "FET";
        }
    }
}
