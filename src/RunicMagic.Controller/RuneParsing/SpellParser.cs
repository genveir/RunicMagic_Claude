using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Controller.RuneParsing
{
    internal static class SpellParser
    {
        public static (int parsedTokens, TemporarySimpleResult<IExecutableStatement>) Parse(string spellString)
        {
            if (string.IsNullOrWhiteSpace(spellString))
            {
                return (0, TemporarySimpleResult<IExecutableStatement>.Fail("Spell string is empty or whitespace"));
            }

            var tokenStream = new TokenStream(spellString);

            var firstRune = tokenStream.First;

            var executableStatementRuneParser = ParserLookup.FindRuneParserByName<IExecutableStatement>(firstRune);

            if (executableStatementRuneParser is null)
            {
                return (0, TemporarySimpleResult<IExecutableStatement>.Fail($"No executable statement rune parser found for '{firstRune}'"));
            }

            var result = executableStatementRuneParser.Parse(tokenStream);
            return (tokenStream.Index + 1, result);
        }
    }
}
