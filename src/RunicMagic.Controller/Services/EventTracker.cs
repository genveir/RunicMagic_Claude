using RunicMagic.Controller.Models;
using RunicMagic.Controller.RuneParsing;
using RunicMagic.World;
using RunicMagic.World.Entities;
using RunicMagic.World.Execution;

namespace RunicMagic.Controller.Services;

public class EventTracker : IWorldEventTracker
{
    private readonly List<WorldEvent> _worldEvents = [];
    private readonly List<ControllerEvent> _controllerEvents = [];
    private readonly List<ParseEvent> _parseEvents = [];
    private readonly HashSet<Entity> _touchedEntities = [];

    public IReadOnlyList<WorldEvent> WorldEvents => _worldEvents;
    public IReadOnlyList<ControllerEvent> ControllerEvents => _controllerEvents;
    public IReadOnlyList<ParseEvent> ParseEvents => _parseEvents;
    public IReadOnlySet<Entity> TouchedEntities => _touchedEntities;

    public void Add(WorldEvent worldEvent)
    {
        _worldEvents.Add(worldEvent);
    }

    public void Add(ControllerEvent controllerEvent)
    {
        _controllerEvents.Add(controllerEvent);
    }

    public void Add(ParseEvent parseEvent)
    {
        _parseEvents.Add(parseEvent);
    }

    public void Track(Entity entity)
    {
        _touchedEntities.Add(entity);
    }

    public bool HasTrackedChanges => _worldEvents.Count + _controllerEvents.Count + _parseEvents.Count + _touchedEntities.Count > 0;
}
