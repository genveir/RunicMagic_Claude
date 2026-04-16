using RunicMagic.World.Runes.EffectRunes;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Controller.RuneParsing.EffectRunes
{
    internal class VUNParser : IRuneParser<IStatement>
    {
        public TemporarySimpleResult<IStatement> Parse(TokenStream tokenStream)
        {
            var toMoveResult = RuneParsingDispatcher.ParseNextRune<IEntitySet>(tokenStream);
            if (!toMoveResult.Succeeded)
            {
                return TemporarySimpleResult<IStatement>.Fail(toMoveResult.ErrorMessage);
            }

            var howFarResult = RuneParsingDispatcher.ParseNextRune<INumber>(tokenStream);
            if (!howFarResult.Succeeded)
            {
                return TemporarySimpleResult<IStatement>.Fail(howFarResult.ErrorMessage);
            }

            var originResult = RuneParsingDispatcher.ParseNextRune<ILocation>(tokenStream, ["PAR", "A"]);
            if (!originResult.Succeeded)
            {
                return TemporarySimpleResult<IStatement>.Fail(originResult.ErrorMessage);
            }

            return TemporarySimpleResult<IStatement>.Succeed(new VUN(toMove: toMoveResult.Value, howFar: howFarResult.Value, origin: originResult.Value));
        }


    }
}
