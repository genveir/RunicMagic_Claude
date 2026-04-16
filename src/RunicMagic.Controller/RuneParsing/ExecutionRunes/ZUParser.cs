using RunicMagic.World.Runes.ExecutionRunes;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Controller.RuneParsing.ExecutionRunes
{
    internal class ZUParser : IRuneParser<IExecutableStatement>
    {
        public TemporarySimpleResult<IExecutableStatement> Parse(TokenStream tokenStream)
        {
            var statementResult = RuneParsingDispatcher.ParseNextRune<IStatement>(tokenStream);

            if (!statementResult.Succeeded)
            {
                return TemporarySimpleResult<IExecutableStatement>.Fail(statementResult.ErrorMessage);
            }

            return TemporarySimpleResult<IExecutableStatement>.Succeed(new ZU(statementResult.Value));
        }
    }
}
