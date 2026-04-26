using RunicMagic.World.Runes.EffectRunes;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Controller.RuneParsing.EffectRunes;

internal class TIORJParser : IRuneParser<IStatement>
{
    public ParsingResult<IStatement> Parse(TokenStream tokenStream)
    {
        var fromResult = RuneParsingDispatcher.ParseNextTaxedEntitySet(tokenStream);
        if (!fromResult.Succeeded)
        {
            return ParsingResult<IStatement>.Fail(fromResult.Error);
        }

        var toResult = RuneParsingDispatcher.ParseNextTaxedEntitySet(tokenStream);
        if (!toResult.Succeeded)
        {
            return ParsingResult<IStatement>.Fail(toResult.Error);
        }

        var amountResult = RuneParsingDispatcher.ParseNextRune<INumber>(tokenStream);
        if (!amountResult.Succeeded)
        {
            return ParsingResult<IStatement>.Fail(amountResult.Error);
        }

        return ParsingResult<IStatement>.Succeed(new TIORJ(from: fromResult.Value, to: toResult.Value, amount: amountResult.Value));
    }
}
