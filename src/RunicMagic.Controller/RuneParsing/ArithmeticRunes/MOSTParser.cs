using RunicMagic.World.Runes.NumberRunes;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Controller.RuneParsing.ArithmeticRunes
{
    internal class MOSTParser : IRuneParser<INumber>
    {
        public ParsingResult<INumber> Parse(TokenStream tokenStream)
        {
            var aResult = RuneParsingDispatcher.ParseNextRune<INumber>(tokenStream);
            if (!aResult.Succeeded)
            {
                return ParsingResult<INumber>.Fail(aResult.Error);
            }

            var result = new MOST(aResult.Value);
            return ParsingResult<INumber>.Succeed(result);
        }
    }
}
