using RunicMagic.World.Execution;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.NumberRunes
{
    // ONE AND A HALF
    public class MOST : INumber
    {
        public INumber A { get; }

        public MOST(INumber a)
        {
            A = a;
        }

        public Number Evaluate(SpellContext context)
        {
            var a = A.Evaluate(context);
            var result = new Number(a.Value * 3 / 2);
            return result;
        }

        public override string ToString()
        {
            var result = $"MOST ( {A} )";
            return result;
        }
    }
}
