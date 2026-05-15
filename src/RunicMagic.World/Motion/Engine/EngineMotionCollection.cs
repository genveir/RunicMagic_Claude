namespace RunicMagic.World.Motion.Engine;

public interface IEngineMotionSink
{
    void AddMotionEffect(IEngineMotionEffect effect);
}

internal class EngineMotionCollection : IEngineMotionSink
{
    private readonly List<IEngineMotionEffect> engineMotionEffects = new();

    public IEnumerable<IEngineMotionEffect> GetAll()
    {
        return engineMotionEffects;
    }

    public void AddMotionEffect(IEngineMotionEffect effect)
    {
        engineMotionEffects.Add(effect);
    }

    public void RemoveAll(Func<IEngineMotionEffect, bool> predicate)
    {
        engineMotionEffects.RemoveAll(new Predicate<IEngineMotionEffect>(predicate));
    }
}
