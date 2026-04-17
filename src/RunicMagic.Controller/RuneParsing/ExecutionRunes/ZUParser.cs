using RunicMagic.World.Runes.ExecutionRunes;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Controller.RuneParsing.ExecutionRunes
{
    internal class ZUParser : IRuneParser<IExecutableStatement>
    {
        public ParsingResult<IExecutableStatement> Parse(TokenStream tokenStream)
        {
            var statementResult = RuneParsingDispatcher.ParseNextRune<IStatement>(tokenStream);

            if (!statementResult.Succeeded)
            {
                return ParsingResult<IExecutableStatement>.Fail(statementResult.Error);
            }

            return ParsingResult<IExecutableStatement>.Succeed(new ZU(statementResult.Value));
        }
    }
}
