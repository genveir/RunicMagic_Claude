using RunicMagic.World.Runes.FilterRunes;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Controller.RuneParsing.FilterRunes
{
    internal class ZYHEParser : IRuneParser<IEntitySet>
    {
        public ParsingResult<IEntitySet> Parse(TokenStream tokenStream)
        {
            var sourceResult = RuneParsingDispatcher.ParseNextRune<IEntitySet>(tokenStream);
            if (!sourceResult.Succeeded)
            {
                return ParsingResult<IEntitySet>.Fail(sourceResult.Error);
            }

            return ParsingResult<IEntitySet>.Succeed(new ZYHE(sourceResult.Value));
        }
    }
}
