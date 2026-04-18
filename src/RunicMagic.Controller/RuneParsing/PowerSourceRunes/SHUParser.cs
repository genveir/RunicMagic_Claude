using RunicMagic.World.Runes.PowerSourceRunes;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Controller.RuneParsing.PowerSourceRunes
{
    internal class SHUParser : IRuneParser<IStatement>
    {
        public ParsingResult<IStatement> Parse(TokenStream tokenStream)
        {
            var sourceResult = RuneParsingDispatcher.ParseNextRune<IEntitySet>(tokenStream);
            if (!sourceResult.Succeeded)
            {
                return ParsingResult<IStatement>.Fail(sourceResult.Error);
            }

            var statementResult = RuneParsingDispatcher.ParseNextRune<IStatement>(tokenStream);
            if (!statementResult.Succeeded)
            {
                return ParsingResult<IStatement>.Fail(statementResult.Error);
            }

            return ParsingResult<IStatement>.Succeed(new SHU(source: sourceResult.Value, statement: statementResult.Value));
        }
    }
}
