using RunicMagic.Controller.Abstractions;
using RunicMagic.Controller.Models;
using RunicMagic.World;
using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;
using RunicMagic.World.Services;
using System.Collections.Concurrent;

namespace RunicMagic.Controller.Services;

internal class PlayerService(
        WorldModel world,
        WorldRenderingService worldRendering,
        SpellCastingService spellCasting,
        RayCastService rayCast) : IPlayerViewInterface, IPlayerOutputSink
{
    private EntityId? casterId = null;

    private readonly List<string> _pendingText = [];
    private readonly ConcurrentQueue<Action> _queue = new();

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

            var prompt = $"({caster.Life.CurrentHitPoints}/{caster.Life.MaxHitPoints}H) ({caster.StructuralIntegrity.CurrentIntegrity}/{caster.StructuralIntegrity.MaxIntegrity}I) >";
            return prompt;
        }
    }

    public Task RegisterInput(string input)
    {
        _queue.Enqueue(() =>
        {
            var responseLines = spellCasting.Cast(input, casterId);
            foreach (var line in responseLines)
                SendText(line);
        });
        return Task.CompletedTask;
    }

    public Task SetCaster(WorldCoordinate worldCoordinate)
    {
        _queue.Enqueue(() =>
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
        });
        return Task.CompletedTask;
    }

    public Task MoveCaster(WorldCoordinate worldCoordinate)
    {
        _queue.Enqueue(() =>
        {
            var caster = CheckForCaster(checkForDeath: true);
            if (caster == null) return;

            TeleportEntityService.Teleport(caster, new Location(worldCoordinate.X, worldCoordinate.Y));
            SendText($"Caster moved to ({worldCoordinate.X}, {worldCoordinate.Y}).");
        });
        return Task.CompletedTask;
    }

    public Task SetPointingDirection(WorldCoordinate worldCoordinate)
    {
        _queue.Enqueue(() =>
        {
            var caster = CheckForCaster(checkForDeath: true);
            if (caster == null) return;

            var to = new Location(worldCoordinate.X, worldCoordinate.Y);
            caster.PointingDirection = Direction.FromPoints(caster.Location, to);
            SendText("Pointing direction set.");
        });
        return Task.CompletedTask;
    }

    public Task SetIndicateTarget(WorldCoordinate worldCoordinate)
    {
        _queue.Enqueue(() =>
        {
            var caster = CheckForCaster(checkForDeath: true);
            if (caster == null) return;

            var entities = world.GetEntitiesAtPoint(worldCoordinate.ToLocation());
            if (entities.Count == 0)
            {
                SendText("Nothing to indicate at that position.");
                return;
            }

            if (entities.Any(e => e.Id == caster.Id))
            {
                caster.IndicateTarget = new IndicateTarget(caster.Id, Direction: null);
                SendText("Indicating self.");
                return;
            }

            var to = worldCoordinate.ToLocation();
            var direction = Direction.FromPoints(caster.Location, to);
            var castResult = rayCast.Cast(caster.Id, caster.Location, direction, skipTranslucent: false);

            if (castResult.HitEntity == null || entities.All(e => e.Id != castResult.HitEntity.Id))
            {
                SendText("Cannot reach that — something is in the way.");
                return;
            }

            var distance = castResult.LocationOfIntersect.GetDistanceTo(caster.Location);
            if (distance > 1000)
            {
                SendText($"{castResult.HitEntity.Label} is out of reach.");
                return;
            }

            caster.IndicateTarget = new IndicateTarget(castResult.HitEntity.Id, direction);
            SendText($"Indicating {castResult.HitEntity.Label}.");
        });
        return Task.CompletedTask;
    }

    public void ReceiveMotionEvents(SpellResult motionResult)
    {
        foreach (var @event in motionResult.Events)
        {
            SendText(SpellEventDescriber.Describe(@event));
        }
    }

    public void SendText(string text)
    {
        _pendingText.Add(text);
    }

    public CommandResult? DrainAndFlush()
    {
        if (_queue.IsEmpty && _pendingText.Count == 0)
            return null;

        while (_queue.TryDequeue(out var action))
            action();

        var entities = worldRendering.GetAllRenderingModels(casterId);
        var result = new CommandResult([.. _pendingText], entities, Prompt);
        _pendingText.Clear();
        return result;
    }

    private Entity? CheckForCaster(bool checkForDeath)
    {
        if (casterId == null)
        {
            SendText("No caster selected.");
            return null;
        }

        var caster = world.Find(casterId.Value);
        if (caster == null)
        {
            SendText("Caster not found in world.");
            return null;
        }

        if (checkForDeath && caster.Life == null)
        {
            SendText("[dead caster] >");
            return null;
        }

        return caster;
    }
}
