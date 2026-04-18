using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Controller.RuneParsing
{
    internal static class SpellParser
    {
        public static (long parsedTokens, ParsingResult<IExecutableStatement>) Parse(string spellString)
        {
            var tokenStream = new TokenStream(spellString);

            var result = RuneParsingDispatcher.ParseNextRune<IExecutableStatement>(tokenStream);

            return ((long)(tokenStream.Index + 1), result);
        }
    }
}
