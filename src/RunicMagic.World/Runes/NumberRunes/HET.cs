using RunicMagic.World.Execution;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.NumberRunes
{
    // ONE
    public class HET : INumber
    {
        public HET() { }

        public Number Evaluate(SpellContext context)
        {
            var result = new Number(1);
            return result;
        }

        public override string ToString()
        {
            return "HET";
        }
    }
}
