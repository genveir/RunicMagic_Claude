using RunicMagic.World.Execution;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.NumberRunes
{
    // MODULO
    public class UIT : INumber
    {
        public INumber A { get; }
        public INumber B { get; }

        public UIT(INumber a, INumber b)
        {
            A = a;
            B = b;
        }

        public Number Evaluate(SpellContext context)
        {
            var a = A.Evaluate(context);
            var b = B.Evaluate(context);
            var result = new Number(a.Value % b.Value);
            return result;
        }

        public override string ToString()
        {
            var result = $"UIT ( {A} , {B} )";
            return result;
        }
    }
}
