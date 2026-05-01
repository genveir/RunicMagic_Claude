namespace RunicMagic.Controller.RuneParsing.DecoratorRunes;

// ACTIVATE — re-evaluate the wrapped expression on every tick (live)
internal class SAParser<T> : IRuneParser<T>
{
    public ParsingResult<T> Parse(TokenStream tokenStream)
    {
        var previousMode = tokenStream.LivenessMode;
        tokenStream.LivenessMode = LivenessMode.Live;
        var result = RuneParsingDispatcher.ParseNextRune<T>(tokenStream);
        tokenStream.LivenessMode = previousMode;

        if (!result.Succeeded)
        {
            return result;
        }
        return ParsingResult<T>.Succeed(result.Value, isLive: true);
    }
}
