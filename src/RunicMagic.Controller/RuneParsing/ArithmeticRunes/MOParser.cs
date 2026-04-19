using RunicMagic.World.Runes.NumberRunes;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Controller.RuneParsing.ArithmeticRunes
{
    internal class MOParser : IRuneParser<INumber>
    {
        public ParsingResult<INumber> Parse(TokenStream tokenStream)
        {
            var aResult = RuneParsingDispatcher.ParseNextRune<INumber>(tokenStream);
            if (!aResult.Succeeded)
            {
                return ParsingResult<INumber>.Fail(aResult.Error);
            }

            var bResult = RuneParsingDispatcher.ParseNextRune<INumber>(tokenStream);
            if (!bResult.Succeeded)
            {
                return ParsingResult<INumber>.Fail(bResult.Error);
            }

            var result = new MO(aResult.Value, bResult.Value);
            return ParsingResult<INumber>.Succeed(result);
        }
    }
}
