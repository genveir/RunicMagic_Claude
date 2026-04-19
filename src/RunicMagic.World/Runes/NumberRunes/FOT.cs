using RunicMagic.World.Execution;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.NumberRunes
{
    // FOURTEEN TO THE FIFTH (537 824)
    public class FOT : INumber
    {
        public FOT() { }

        public Number Evaluate(SpellContext context)
        {
            var result = new Number(537824);
            return result;
        }

        public override string ToString()
        {
            return "FOT";
        }
    }
}
