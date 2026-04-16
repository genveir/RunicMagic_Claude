using RunicMagic.World.Runes.NumberRunes;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Controller.RuneParsing.NumberRunes
{
    internal class FOTIRParser : IRuneParser<INumber>
    {
        public TemporarySimpleResult<INumber> Parse(TokenStream tokenStream)
        {
            var multiplicandResult = RuneParsingDispatcher.ParseNextRune<INumber>(tokenStream);

            if (!multiplicandResult.Succeeded)
            {
                return TemporarySimpleResult<INumber>.Fail(multiplicandResult.ErrorMessage);
            }

            var result = new FOTIR(multiplicandResult.Value);
            return TemporarySimpleResult<INumber>.Succeed(result);
        }
    }
}
