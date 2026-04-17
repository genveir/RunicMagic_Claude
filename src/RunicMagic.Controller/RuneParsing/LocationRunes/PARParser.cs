using RunicMagic.World.Runes.LocationRunes;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Controller.RuneParsing.LocationRunes
{
    internal class PARParser : IRuneParser<ILocation>
    {
        public ParsingResult<ILocation> Parse(TokenStream tokenStream)
        {
            var entitySetResult = RuneParsingDispatcher.ParseNextRune<IEntitySet>(tokenStream);

            if (!entitySetResult.Succeeded)
            {
                return ParsingResult<ILocation>.Fail(entitySetResult.Error);
            }

            return ParsingResult<ILocation>.Succeed(new PAR(entitySetResult.Value));
        }
    }
}
