using RunicMagic.World.Runes.LocationRunes;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Controller.RuneParsing.LocationRunes
{
    internal class GERParser : IRuneParser<ILocation>
    {
        public ParsingResult<ILocation> Parse(TokenStream tokenStream)
        {
            var entitySetResult = RuneParsingDispatcher.ParseNextRune<IEntitySet>(tokenStream);

            if (!entitySetResult.Succeeded)
            {
                return ParsingResult<ILocation>.Fail(entitySetResult.Error);
            }

            return ParsingResult<ILocation>.Succeed(new GER(entitySetResult.Value));
        }
    }
}
