using RunicMagic.World.Runes.InvocationRunes;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Controller.RuneParsing.InvocationRunes
{
    internal class CRIYRParser : IRuneParser<IStatement>
    {
        public ParsingResult<IStatement> Parse(TokenStream tokenStream)
        {
            var targetResult = RuneParsingDispatcher.ParseNextTaxedEntitySet(tokenStream);
            if (!targetResult.Succeeded)
            {
                return ParsingResult<IStatement>.Fail(targetResult.Error);
            }

            return ParsingResult<IStatement>.Succeed(new CRIYR(target: targetResult.Value));
        }
    }
}
