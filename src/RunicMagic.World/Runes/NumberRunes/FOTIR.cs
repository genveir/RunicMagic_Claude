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

        public override string ToString()
        {
            var result = $"FOTIR ( {Multiplicand} )";
            return result;
        }
    }
}
