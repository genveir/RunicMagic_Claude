using RunicMagic.World.Execution;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.NumberRunes
{
    // FOURTEEN
    public class HOT : INumber
    {
        public HOT() { }

        public Number Evaluate(SpellContext context)
        {
            var result = new Number(14);
            return result;
        }

        public override string ToString()
        {
            return "HOT";
        }
    }
}
