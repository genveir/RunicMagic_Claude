using RunicMagic.World.Runes.DebugRunes;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Controller.RuneParsing.DebugRunes
{
    internal class DETAILSParser : IRuneParser<IStatement>
    {
        public ParsingResult<IStatement> Parse(TokenStream tokenStream)
        {
            var entitiesResult = RuneParsingDispatcher.ParseNextRune<IEntitySet>(tokenStream, ["DAN"]);
            if (!entitiesResult.Succeeded)
            {
                return ParsingResult<IStatement>.Fail(entitiesResult.Error);
            }

            var details = new DETAILS(entitiesResult.Value);
            return ParsingResult<IStatement>.Succeed(details);
        }
    }
}
