using RunicMagic.World.Execution;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.NumberRunes
{
    // HALVE
    public class DEID : INumber
    {
        public INumber A { get; }

        public DEID(INumber a)
        {
            A = a;
        }

        public Number Evaluate(SpellContext context)
        {
            var a = A.Evaluate(context);
            var result = new Number(a.Value / 2);
            return result;
        }

        public override string ToString()
        {
            var result = $"DEID ( {A} )";
            return result;
        }
    }
}
