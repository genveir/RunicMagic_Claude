using RunicMagic.World.Runes.EntityReferenceRunes;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Controller.RuneParsing.EntityReferenceRunes
{
    internal class OHParser : IRuneParser<IEntitySet>
    {
        public TemporarySimpleResult<IEntitySet> Parse(TokenStream tokenStream)
        {
            return TemporarySimpleResult<IEntitySet>.Succeed(new OH());
        }
    }
}
