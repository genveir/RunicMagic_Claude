using RunicMagic.World.Runes.EntitySetRunes;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Controller.RuneParsing.EntitySetRunes
{
    internal class PAParser : IRuneParser<IEntitySet>
    {
        public ParsingResult<IEntitySet> Parse(TokenStream tokenStream)
        {
            var toGetScopeOfResult = RuneParsingDispatcher.ParseNextRune<IEntitySet>(tokenStream, defaultTokens: ["OH"]);
            if (!toGetScopeOfResult.Succeeded)
            {
                return ParsingResult<IEntitySet>.Fail(toGetScopeOfResult.Error);
            }

            var result = new PA(toGetScopeOfResult.Value);
            return ParsingResult<IEntitySet>.Succeed(result);
        }
    }
}
