namespace RunicMagic.World.Motion.Engine;

internal class EngineMotionCollection
{
    private readonly List<IEngineMotionEffect> _engineMotionEffects = new();

    public IEnumerable<IEngineMotionEffect> GetAll()
    {
        return _engineMotionEffects;
    }

    public void AddMotionEffect(IEngineMotionEffect effect)
    {
        _engineMotionEffects.Add(effect);
    }

    public void RemoveAll(Func<IEngineMotionEffect, bool> predicate)
    {
        _engineMotionEffects.RemoveAll(new Predicate<IEngineMotionEffect>(predicate));
    }
}
