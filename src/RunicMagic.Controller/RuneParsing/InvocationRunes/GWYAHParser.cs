using RunicMagic.World.Runes.InvocationRunes;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Controller.RuneParsing.InvocationRunes
{
    internal class GWYAHParser : IRuneParser<IStatement>
    {
        public ParsingResult<IStatement> Parse(TokenStream tokenStream)
        {
            var targetResult = RuneParsingDispatcher.ParseNextTaxedEntitySet(tokenStream);
            if (!targetResult.Succeeded)
            {
                return ParsingResult<IStatement>.Fail(targetResult.Error);
            }

            return ParsingResult<IStatement>.Succeed(new GWYAH(target: targetResult.Value));
        }
    }
}
