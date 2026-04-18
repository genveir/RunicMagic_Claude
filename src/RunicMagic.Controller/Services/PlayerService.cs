using RunicMagic.Controller.Abstractions;
using RunicMagic.Controller.Models;
using RunicMagic.World;

namespace RunicMagic.Controller.Services;

internal class PlayerService(
        WorldModel world,
        WorldRenderingService worldRendering,
        SpellCastingService spellCasting,
        TeleportEntityService teleport) : IPlayerViewInterface, IPlayerOutputSink
{
    private EntityId? casterId = null;

    private readonly List<string> _pendingText = [];
    private readonly List<EntityRenderingModel> _pendingEntities = [];

    public string Prompt => ">";

    public Task<CommandResult> RegisterInput(string input)
    {
        var responseLines = spellCasting.Cast(input, casterId);
        foreach (var line in responseLines)
        {
            SendText(line);
        }

        return Task.FromResult(FlushPendingOutputs());
    }

    public Task<CommandResult> SetCaster(WorldCoordinate worldCoordinate)
    {
        var entities = world.GetEntitiesAtPoint(worldCoordinate.X, worldCoordinate.Y)
            .Where(e => e.HasAgency)
            .ToList();

        if (entities.Count == 0)
        {
            SendText($"No entities with agency found at ({worldCoordinate.X}, {worldCoordinate.Y}).");
        }
        else if (entities.Count > 1)
        {
            SendText($"Multiple entities with agency found at ({worldCoordinate.X}, {worldCoordinate.Y}). Unable to resolve a caster.");
        }
        else
        {
            var casterEntity = entities[0];
            casterId = casterEntity.Id;
            SendText($"Caster set to entity {casterEntity.Label} at ({worldCoordinate.X}, {worldCoordinate.Y}).");
        }

        return Task.FromResult(FlushPendingOutputs());
    }

    public Task<CommandResult> MoveCaster(WorldCoordinate worldCoordinate)
    {
        if (casterId == null)
        {
            SendText("No caster selected.");
            return Task.FromResult(FlushPendingOutputs());
        }

        var casterEntity = world.Find(casterId.Value);
        if (casterEntity == null)
        {
            SendText("Caster not found in world.");
            return Task.FromResult(FlushPendingOutputs());
        }

        teleport.Teleport(casterEntity, worldCoordinate.X, worldCoordinate.Y);
        SendText($"Caster moved to ({worldCoordinate.X}, {worldCoordinate.Y}).");
        return Task.FromResult(FlushPendingOutputs());
    }

    public void SendText(string text)
    {
        _pendingText.Add(text);
    }

    public void SendWorldEntities()
    {
        _pendingEntities.AddRange(worldRendering.GetAllRenderingModels(casterId));
    }

    private CommandResult FlushPendingOutputs()
    {
        SendWorldEntities();

        var result = new CommandResult([.. _pendingText], [.. _pendingEntities]);
        _pendingText.Clear();
        _pendingEntities.Clear();
        return result;
    }
}
