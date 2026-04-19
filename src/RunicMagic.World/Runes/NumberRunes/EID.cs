using RunicMagic.World.Execution;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.NumberRunes
{
    // INTEGER DIVIDE
    public class EID : INumber
    {
        public INumber A { get; }
        public INumber B { get; }

        public EID(INumber a, INumber b)
        {
            A = a;
            B = b;
        }

        public Number Evaluate(SpellContext context)
        {
            var a = A.Evaluate(context);
            var b = B.Evaluate(context);
            var result = new Number(a.Value / b.Value);
            return result;
        }

        public override string ToString()
        {
            var result = $"EID ( {A} , {B} )";
            return result;
        }
    }
}
