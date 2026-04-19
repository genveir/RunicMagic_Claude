using RunicMagic.World.Execution;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.NumberRunes
{
    // SEVEN
    public class SET : INumber
    {
        public SET() { }

        public Number Evaluate(SpellContext context)
        {
            var result = new Number(7);
            return result;
        }

        public override string ToString()
        {
            return "SET";
        }
    }
}
