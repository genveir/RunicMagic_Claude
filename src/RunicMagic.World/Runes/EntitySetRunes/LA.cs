using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.EntitySetRunes
{
    // SCOPE OF
    public class LA : IEntitySet
    {
        public IEntitySet ToGetScopeOf { get; }

        public LA(IEntitySet toGetScopeOf)
        {
            ToGetScopeOf = toGetScopeOf;
        }
    }
}
