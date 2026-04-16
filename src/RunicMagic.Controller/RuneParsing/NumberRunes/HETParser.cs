using RunicMagic.World.Runes.NumberRunes;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Controller.RuneParsing.NumberRunes
{
    internal class HETParser : IRuneParser<INumber>
    {
        public TemporarySimpleResult<INumber> Parse(TokenStream tokenStream)
        {
            return TemporarySimpleResult<INumber>.Succeed(new HET());
        }
    }
}
