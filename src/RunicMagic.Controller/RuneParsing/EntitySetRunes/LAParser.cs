using RunicMagic.World.Runes.EntitySetRunes;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Controller.RuneParsing.EntitySetRunes
{
    internal class LAParser : IRuneParser<IEntitySet>
    {
        public TemporarySimpleResult<IEntitySet> Parse(TokenStream tokenStream)
        {
            var toGetScopeOfResult = RuneParsingDispatcher.ParseNextRune<IEntitySet>(tokenStream, defaultTokens: ["OH"]);

            if (!toGetScopeOfResult.Succeeded)
            {
                return TemporarySimpleResult<IEntitySet>.Fail(toGetScopeOfResult.ErrorMessage);
            }

            var result = new LA(toGetScopeOfResult.Value);
            return TemporarySimpleResult<IEntitySet>.Succeed(result);
        }
    }
}
