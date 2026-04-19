using RunicMagic.World.Runes.EffectRunes;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Controller.RuneParsing.EffectRunes
{
    internal class VARParser : IRuneParser<IStatement>
    {
        public ParsingResult<IStatement> Parse(TokenStream tokenStream)
        {
            var toMoveResult = RuneParsingDispatcher.ParseNextTaxedEntitySet(tokenStream);
            if (!toMoveResult.Succeeded)
            {
                return ParsingResult<IStatement>.Fail(toMoveResult.Error);
            }

            var howFarResult = RuneParsingDispatcher.ParseNextRune<INumber>(tokenStream);
            if (!howFarResult.Succeeded)
            {
                return ParsingResult<IStatement>.Fail(howFarResult.Error);
            }

            var originResult = RuneParsingDispatcher.ParseNextRune<ILocation>(tokenStream, ["PAR", "OH"]);
            if (!originResult.Succeeded)
            {
                return ParsingResult<IStatement>.Fail(originResult.Error);
            }

            return ParsingResult<IStatement>.Succeed(new VAR(toMove: toMoveResult.Value, howFar: howFarResult.Value, origin: originResult.Value));
        }
    }
}
