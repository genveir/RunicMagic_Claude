using RunicMagic.World.Execution;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.NumberRunes
{
    // FOURTEEN TO THE SEVENTH (105 413 504)
    public class SOT : INumber
    {
        public SOT() { }

        public Number Evaluate(SpellContext context)
        {
            var result = new Number(105413504);
            return result;
        }

        public override string ToString()
        {
            return "SOT";
        }
    }
}
