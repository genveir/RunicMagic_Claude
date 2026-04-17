using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Controller.RuneParsing
{
    internal static class SpellParser
    {
        public static (int parsedTokens, TemporarySimpleResult<IExecutableStatement>) Parse(string spellString)
        {
            var tokenStream = new TokenStream(spellString);

            var result = RuneParsingDispatcher.ParseNextRune<IExecutableStatement>(tokenStream);

            return (tokenStream.Index + 1, result);
        }
    }
}
