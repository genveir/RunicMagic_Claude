using RunicMagic.World.Runes.LocationRunes;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Controller.RuneParsing.LocationRunes
{
    internal class PARParser : IRuneParser<ILocation>
    {
        public TemporarySimpleResult<ILocation> Parse(TokenStream tokenStream)
        {
            var entitySetResult = RuneParsingDispatcher.ParseNextRune<IEntitySet>(tokenStream);

            if (!entitySetResult.Succeeded)
            {
                return TemporarySimpleResult<ILocation>.Fail(entitySetResult.ErrorMessage);
            }

            return TemporarySimpleResult<ILocation>.Succeed(new PAR(entitySetResult.Value));
        }
    }
}
