using RunicMagic.World.Runes.NumberRunes;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Controller.RuneParsing.NumberRunes
{
    internal class HETParser : IRuneParser<INumber>
    {
        public ParsingResult<INumber> Parse(TokenStream tokenStream)
        {
            return ParsingResult<INumber>.Succeed(new HET());
        }
    }
}
