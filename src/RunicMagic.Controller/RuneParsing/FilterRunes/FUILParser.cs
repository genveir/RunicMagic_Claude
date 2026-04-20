using RunicMagic.World.Runes.FilterRunes;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Controller.RuneParsing.FilterRunes
{
    internal class FUILParser : IRuneParser<IEntitySet>
    {
        public ParsingResult<IEntitySet> Parse(TokenStream tokenStream)
        {
            var sourceResult = RuneParsingDispatcher.ParseNextRune<IEntitySet>(tokenStream);
            if (!sourceResult.Succeeded)
            {
                return ParsingResult<IEntitySet>.Fail(sourceResult.Error);
            }

            var lowerResult = RuneParsingDispatcher.ParseNextRune<INumber>(tokenStream);
            if (!lowerResult.Succeeded)
            {
                return ParsingResult<IEntitySet>.Fail(lowerResult.Error);
            }

            var upperResult = RuneParsingDispatcher.ParseNextRune<INumber>(tokenStream);
            if (!upperResult.Succeeded)
            {
                return ParsingResult<IEntitySet>.Fail(upperResult.Error);
            }

            return ParsingResult<IEntitySet>.Succeed(new FUIL(source: sourceResult.Value, lower: lowerResult.Value, upper: upperResult.Value));
        }
    }
}
