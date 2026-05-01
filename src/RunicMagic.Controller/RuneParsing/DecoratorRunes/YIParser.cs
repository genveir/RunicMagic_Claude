namespace RunicMagic.Controller.RuneParsing.DecoratorRunes;

// CALCIFY — evaluate the wrapped expression once and cache the result
internal class YIParser<T> : IRuneParser<T>
{
    public ParsingResult<T> Parse(TokenStream tokenStream)
    {
        var previousMode = tokenStream.LivenessMode;
        tokenStream.LivenessMode = LivenessMode.Calcified;
        var result = RuneParsingDispatcher.ParseNextRune<T>(tokenStream);
        tokenStream.LivenessMode = previousMode;

        if (!result.Succeeded)
        {
            return result;
        }
        return ParsingResult<T>.Succeed(result.Value, isLive: false);
    }
}
