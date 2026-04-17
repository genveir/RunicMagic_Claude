using RunicMagic.World.Runes.EntityReferenceRunes;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Controller.RuneParsing.EntityReferenceRunes
{
    internal class AParser : IRuneParser<IEntitySet>
    {
        public ParsingResult<IEntitySet> Parse(TokenStream tokenStream)
        {
            return ParsingResult<IEntitySet>.Succeed(new A());
        }
    }
}
