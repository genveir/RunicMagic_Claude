using RunicMagic.Controller.Abstractions;
using RunicMagic.Controller.Models;
using RunicMagic.World;
using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;

namespace RunicMagic.Controller.Services;

internal class PlayerService(
        WorldModel world,
        WorldRenderingService worldRendering,
        SpellCastingService spellCasting,
        TeleportEntityService teleport,
        RayCastService rayCast) : IPlayerViewInterface, IPlayerOutputSink
{
    private EntityId? casterId = null;

    private readonly List<string> _pendingText = [];
    private readonly List<EntityRenderingModel> _pendingEntities = [];

    public string Prompt
    {
        get
        {
            if (casterId == null)
            {
                return "[no caster] >";
            }

            var caster = world.Find(casterId.Value);
            if (caster?.Life == null)
            {
                return "[dead caster] >";
            }

            var prompt = $"({caster.Life.CurrentHitPoints}/{caster.Life.MaxHitPoints}) >";
            return prompt;
        }
    }

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
        var entities = world.GetEntitiesAtPoint(worldCoordinate.ToLocation())
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
        var (caster, error) = CheckForCaster(checkForDeath: true);
        if (error != null) return Task.FromResult(error);
        if (caster == null) throw new InvalidOperationException("Unexpected null caster after check.");

        teleport.Teleport(caster, new Location(worldCoordinate.X, worldCoordinate.Y));
        SendText($"Caster moved to ({worldCoordinate.X}, {worldCoordinate.Y}).");

        return Task.FromResult(FlushPendingOutputs());
    }

    public Task<CommandResult> SetPointingDirection(WorldCoordinate worldCoordinate)
    {
        var (caster, error) = CheckForCaster(checkForDeath: true);
        if (error != null) return Task.FromResult(error);
        if (caster == null) throw new InvalidOperationException("Unexpected null caster after check.");

        var to = new Location(worldCoordinate.X, worldCoordinate.Y);
        caster.PointingDirection = Direction.FromPoints(caster.Location, to);
        SendText("Pointing direction set.");

        return Task.FromResult(FlushPendingOutputs());
    }

    public Task<CommandResult> SetIndicateTarget(WorldCoordinate worldCoordinate)
    {
        var (caster, error) = CheckForCaster(checkForDeath: true);
        if (error != null) return Task.FromResult(error);
        if (caster == null) throw new InvalidOperationException("Unexpected null caster after check.");

        var entities = world.GetEntitiesAtPoint(worldCoordinate.ToLocation());
        if (entities.Count == 0)
        {
            SendText("Nothing to indicate at that position.");
            return Task.FromResult(FlushPendingOutputs());
        }

        if (entities.Any(e => e.Id == caster.Id))
        {
            caster.IndicateTarget = new IndicateTarget(caster.Id, Direction: null);
            SendText("Indicating self.");
            return Task.FromResult(FlushPendingOutputs());
        }

        var to = worldCoordinate.ToLocation();
        var direction = Direction.FromPoints(caster.Location, to);
        var castResult = rayCast.Cast(caster.Id, caster.Location, direction, skipTranslucent: false);

        if (castResult.HitEntity == null || entities.All(e => e.Id != castResult.HitEntity.Id))
        {
            SendText("Cannot reach that — something is in the way.");
            return Task.FromResult(FlushPendingOutputs());
        }

        var distance = castResult.LocationOfIntersect.GetDistanceTo(caster.Location);
        if (distance > 1000)
        {
            SendText($"{castResult.HitEntity.Label} is out of reach.");
            return Task.FromResult(FlushPendingOutputs());
        }

        caster.IndicateTarget = new IndicateTarget(castResult.HitEntity.Id, direction);
        SendText($"Indicating {castResult.HitEntity.Label}.");
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

    private (Entity? caster, CommandResult? error) CheckForCaster(bool checkForDeath)
    {
        (Entity?, CommandResult?) result = (null, null);

        if (casterId == null)
        {
            SendText("No caster selected.");
            result = (null, FlushPendingOutputs());
        }
        else
        {
            var caster = world.Find(casterId.Value);
            if (caster == null)
            {
                SendText("Caster not found in world.");
                result = (null, FlushPendingOutputs());
            }
            else if (checkForDeath && caster.Life == null)
            {
                SendText("[dead caster] >");
                result = (null, FlushPendingOutputs());
            }
            else
            {
                result = (caster, null);
            }
        }

        return result;
    }

    private CommandResult FlushPendingOutputs()
    {
        SendWorldEntities();

        var result = new CommandResult([.. _pendingText], [.. _pendingEntities], Prompt);
        _pendingText.Clear();
        _pendingEntities.Clear();
        return result;
    }
}
