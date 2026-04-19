using RunicMagic.World.Execution;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.NumberRunes
{
    // TWO
    public class DET : INumber
    {
        public DET() { }

        public Number Evaluate(SpellContext context)
        {
            var result = new Number(2);
            return result;
        }

        public override string ToString()
        {
            return "DET";
        }
    }
}
