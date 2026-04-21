using RunicMagic.World.Runes.RuneTypes;
using RunicMagic.World.Runes.SetOperationRunes;

namespace RunicMagic.Controller.RuneParsing.SetOperationRunes
{
    internal class ANParser : IRuneParser<IEntitySet>
    {
        public ParsingResult<IEntitySet> Parse(TokenStream tokenStream)
        {
            var leftResult = RuneParsingDispatcher.ParseNextRune<IEntitySet>(tokenStream);
            if (!leftResult.Succeeded)
            {
                return ParsingResult<IEntitySet>.Fail(leftResult.Error);
            }

            var rightResult = RuneParsingDispatcher.ParseNextRune<IEntitySet>(tokenStream);
            if (!rightResult.Succeeded)
            {
                return ParsingResult<IEntitySet>.Fail(rightResult.Error);
            }

            return ParsingResult<IEntitySet>.Succeed(new AN(left: leftResult.Value, right: rightResult.Value));
        }
    }
}
