using RunicMagic.World.Execution;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.NumberRunes
{
    // ADD
    public class MO : INumber
    {
        public INumber A { get; }
        public INumber B { get; }

        public MO(INumber a, INumber b)
        {
            A = a;
            B = b;
        }

        public Number Evaluate(SpellContext context)
        {
            var a = A.Evaluate(context);
            var b = B.Evaluate(context);
            var result = new Number(a.Value + b.Value);
            return result;
        }

        public override string ToString()
        {
            var result = $"MO ( {A} , {B} )";
            return result;
        }
    }
}
