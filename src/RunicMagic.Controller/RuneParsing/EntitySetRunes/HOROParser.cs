using RunicMagic.World.Runes.EntitySetRunes;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Controller.RuneParsing.EntitySetRunes
{
    internal class HOROParser : IRuneParser<IEntitySet>
    {
        public ParsingResult<IEntitySet> Parse(TokenStream tokenStream)
        {
            var howFarResult = RuneParsingDispatcher.ParseNextRune<INumber>(tokenStream);
            if (!howFarResult.Succeeded)
            {
                return ParsingResult<IEntitySet>.Fail(howFarResult.Error);
            }

            var originResult = RuneParsingDispatcher.ParseNextRune<ILocation>(tokenStream, ["PAR", "OH"]);
            if (!originResult.Succeeded)
            {
                return ParsingResult<IEntitySet>.Fail(originResult.Error);
            }

            return ParsingResult<IEntitySet>.Succeed(new HORO(howFar: howFarResult.Value, origin: originResult.Value));
        }
    }
}
