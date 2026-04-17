using RunicMagic.World.Execution;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.NumberRunes
{
    // TIMES FOURTEEN
    public class FOTIR : INumber
    {
        public INumber Multiplicand { get; }

        public FOTIR(INumber multiplicand)
        {
            Multiplicand = multiplicand;
        }

        public Number Evaluate(SpellContext context)
        {
            var multiplicand = Multiplicand.Evaluate(context);
            var result = new Number(multiplicand.Value * 14);
            return result;
        }

        public override string ToString()
        {
            var result = $"FOTIR ( {Multiplicand} )";
            return result;
        }
    }
}
