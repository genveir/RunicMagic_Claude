using RunicMagic.World.Runes.EffectRunes;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Controller.RuneParsing.EffectRunes
{
    internal class CJIRParser : IRuneParser<IStatement>
    {
        public ParsingResult<IStatement> Parse(TokenStream tokenStream)
        {
            tokenStream.OpenRecordingWindow();
            var toRotateResult = RuneParsingDispatcher.ParseNextTaxedEntitySet(tokenStream);
            var toRotateTokens = tokenStream.CloseRecordingWindow();
            if (!toRotateResult.Succeeded)
            {
                return ParsingResult<IStatement>.Fail(toRotateResult.Error);
            }

            var howMuchResult = RuneParsingDispatcher.ParseNextRune<INumber>(tokenStream);
            if (!howMuchResult.Succeeded)
            {
                return ParsingResult<IStatement>.Fail(howMuchResult.Error);
            }

            var originDefault = new[] { "PAR" }.Concat(toRotateTokens).ToArray();
            var originResult = RuneParsingDispatcher.ParseNextRune<ILocation>(tokenStream, originDefault);
            if (!originResult.Succeeded)
            {
                return ParsingResult<IStatement>.Fail(originResult.Error);
            }

            return ParsingResult<IStatement>.Succeed(new CJIR(toRotate: toRotateResult.Value, howMuch: howMuchResult.Value, origin: originResult.Value));
        }
    }
}
