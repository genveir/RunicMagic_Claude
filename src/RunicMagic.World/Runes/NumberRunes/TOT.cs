using RunicMagic.World.Execution;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.NumberRunes
{
    // FOURTEEN CUBED (2 744)
    public class TOT : INumber
    {
        public TOT() { }

        public Number Evaluate(SpellContext context)
        {
            var result = new Number(2744);
            return result;
        }

        public override string ToString()
        {
            return "TOT";
        }
    }
}
