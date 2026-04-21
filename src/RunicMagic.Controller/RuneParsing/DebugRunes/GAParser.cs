using RunicMagic.World.Runes.DebugRunes;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Controller.RuneParsing.DebugRunes
{
    internal class GAParser : IRuneParser<IEntitySet>
    {
        public ParsingResult<IEntitySet> Parse(TokenStream tokenStream)
        {
            return ParsingResult<IEntitySet>.Succeed(new GA());
        }
    }
}
