using RunicMagic.World.Runes.EntitySetRunes;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Controller.RuneParsing.EntitySetRunes;

internal class GAParser : IRuneParser<IEntitySet>
{
    public ParsingResult<IEntitySet> Parse(TokenStream tokenStream)
    {
        return ParsingResult<IEntitySet>.Succeed(new GA());
    }
}
