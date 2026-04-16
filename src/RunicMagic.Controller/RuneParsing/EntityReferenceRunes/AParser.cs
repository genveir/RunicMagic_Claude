using RunicMagic.World.Runes.EntityReferenceRunes;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Controller.RuneParsing.EntityReferenceRunes
{
    internal class AParser : IRuneParser<IEntitySet>
    {
        public TemporarySimpleResult<IEntitySet> Parse(TokenStream tokenStream)
        {
            return TemporarySimpleResult<IEntitySet>.Succeed(new A());
        }
    }
}
