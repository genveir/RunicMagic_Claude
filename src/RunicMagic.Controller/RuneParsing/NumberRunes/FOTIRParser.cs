using RunicMagic.World.Runes.NumberRunes;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Controller.RuneParsing.NumberRunes
{
    internal class FOTIRParser : IRuneParser<INumber>
    {
        public ParsingResult<INumber> Parse(TokenStream tokenStream)
        {
            var multiplicandResult = RuneParsingDispatcher.ParseNextRune<INumber>(tokenStream);

            if (!multiplicandResult.Succeeded)
            {
                return ParsingResult<INumber>.Fail(multiplicandResult.Error);
            }

            var result = new FOTIR(multiplicandResult.Value);
            return ParsingResult<INumber>.Succeed(result);
        }
    }
}
