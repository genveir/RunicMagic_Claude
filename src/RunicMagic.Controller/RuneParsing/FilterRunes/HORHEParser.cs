using RunicMagic.World.Runes.FilterRunes;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Controller.RuneParsing.FilterRunes
{
    internal class HORHEParser : IRuneParser<IEntitySet>
    {
        public ParsingResult<IEntitySet> Parse(TokenStream tokenStream)
        {
            var sourceResult = RuneParsingDispatcher.ParseNextRune<IEntitySet>(tokenStream);
            if (!sourceResult.Succeeded)
            {
                return ParsingResult<IEntitySet>.Fail(sourceResult.Error);
            }

            var originResult = RuneParsingDispatcher.ParseNextRune<ILocation>(tokenStream, defaultTokens: ["PAR", "OH"]);
            if (!originResult.Succeeded)
            {
                return ParsingResult<IEntitySet>.Fail(originResult.Error);
            }

            return ParsingResult<IEntitySet>.Succeed(new HORHE(source: sourceResult.Value, origin: originResult.Value));
        }
    }
}
