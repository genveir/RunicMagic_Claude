using RunicMagic.World.Runes.EntitySetRunes;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Controller.RuneParsing.EntitySetRunes;

internal class CJODANParser : IRuneParser<IEntitySet>
{
    public ParsingResult<IEntitySet> Parse(TokenStream tokenStream)
    {
        var rangeResult = RuneParsingDispatcher.ParseNextRune<INumber>(tokenStream);
        if (!rangeResult.Succeeded)
        {
            return ParsingResult<IEntitySet>.Fail(rangeResult.Error);
        }

        var halfAngleResult = RuneParsingDispatcher.ParseNextRune<INumber>(tokenStream, ["DOT"]);
        if (!halfAngleResult.Succeeded)
        {
            return ParsingResult<IEntitySet>.Fail(halfAngleResult.Error);
        }

        return ParsingResult<IEntitySet>.Succeed(
            new CJODAN(range: rangeResult.Value, halfAngle: halfAngleResult.Value),
            isLive: rangeResult.IsLive || halfAngleResult.IsLive);
    }
}
