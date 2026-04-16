using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.World.Runes.EffectRunes
{
    // PUSH
    public class VUN : IStatement
    {
        public IEntitySet ToMove { get; }
        public INumber HowFar { get; }
        public ILocation Origin { get; }

        public VUN(IEntitySet toMove, INumber howFar, ILocation origin)
        {
            ToMove = toMove;
            HowFar = howFar;
            Origin = origin;
        }
    }
}
