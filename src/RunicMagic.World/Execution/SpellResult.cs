namespace RunicMagic.World.Execution;

public class SpellResult
{
    private readonly List<SpellEvent> _events = new();

    public IReadOnlyList<SpellEvent> Events
    {
        get
        {
            return _events;
        }
    }

    public void Add(SpellEvent @event)
    {
        _events.Add(@event);
    }
}
