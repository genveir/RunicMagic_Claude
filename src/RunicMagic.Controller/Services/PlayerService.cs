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
        SpellCastingService spellCasting,
        RayCastService rayCast) : IPlayerViewInterface
{
    private EntityId? casterId = null;

    private readonly ConcurrentQueue<Action<EventTracker>> _queue = new();

    public Task RegisterInput(string input)
    {
        _queue.Enqueue((eventTracker) =>
        {
            spellCasting.Cast(input, casterId, eventTracker);
        });
        return Task.CompletedTask;
    }

    public Task SetCaster(WorldCoordinate worldCoordinate)
    {
        _queue.Enqueue((eventTracker) =>
        {
            var entities = world.GetEntitiesAtPoint(worldCoordinate.ToLocation())
                .Where(e => e.HasAgency)
                .ToList();

            if (entities.Count == 0)
            {
                eventTracker.Add(new NoEntitiesWithAgencyFoundEvent());
            }
            else if (entities.Count > 1)
            {
                eventTracker.Add(new MultipleEntitiesWithAgencyFoundEvent());
            }
            else
            {
                var casterEntity = entities[0];
                casterId = casterEntity.Id;
                eventTracker.Add(new CasterSetEvent(casterEntity));
            }
        });
        return Task.CompletedTask;
    }

    public Task MoveCaster(WorldCoordinate worldCoordinate)
    {
        _queue.Enqueue((eventTracker) =>
        {
            var caster = CheckForCaster(eventTracker, checkForDeath: true);
            if (caster == null) return;

            TeleportEntityService.Teleport(caster, new Location(worldCoordinate.X, worldCoordinate.Y));
            eventTracker.Add(new CasterMovedEvent());
        });
        return Task.CompletedTask;
    }

    public Task SetPointingDirection(WorldCoordinate worldCoordinate)
    {
        _queue.Enqueue((eventTracker) =>
        {
            var caster = CheckForCaster(eventTracker, checkForDeath: true);
            if (caster == null) return;

            var to = new Location(worldCoordinate.X, worldCoordinate.Y);
            caster.PointingDirection = Direction.FromPoints(caster.Location, to);
            eventTracker.Add(new PointingDirectionSetEvent());
        });
        return Task.CompletedTask;
    }

    public Task SetIndicateTarget(WorldCoordinate worldCoordinate)
    {
        _queue.Enqueue((eventTracker) =>
        {
            var caster = CheckForCaster(eventTracker, checkForDeath: true);
            if (caster == null) return;

            var entities = world.GetEntitiesAtPoint(worldCoordinate.ToLocation());
            if (entities.Count == 0)
            {
                eventTracker.Add(new NothingToIndicateEvent());
                return;
            }

            if (entities.Any(e => e.Id == caster.Id))
            {
                caster.IndicateTarget = new IndicateTarget(caster.Id, Direction: null);
                eventTracker.Add(new IndicatingEvent(caster));
                return;
            }

            var to = worldCoordinate.ToLocation();
            var direction = Direction.FromPoints(caster.Location, to);
            var castResult = rayCast.Cast(caster.Id, caster.Location, direction, skipTranslucent: false);

            if (castResult.HitEntity == null || entities.All(e => e.Id != castResult.HitEntity.Id))
            {
                eventTracker.Add(new IndicateTargetBlockedEvent());
                return;
            }

            var distance = castResult.LocationOfIntersect.GetDistanceTo(caster.Location);
            if (distance > 1000)
            {
                eventTracker.Add(new IndicateTargetOutOfReachEvent(castResult.HitEntity));
                return;
            }

            caster.IndicateTarget = new IndicateTarget(castResult.HitEntity.Id, direction);
            eventTracker.Add(new IndicatingEvent(castResult.HitEntity));
        });
        return Task.CompletedTask;
    }

    public EntityId? DrainAndFlush(EventTracker eventTracker)
    {
        while (_queue.TryDequeue(out var action))
            action(eventTracker);

        return casterId;
    }

    private Entity? CheckForCaster(EventTracker eventTracker, bool checkForDeath)
    {
        if (casterId == null)
        {
            eventTracker.Add(new NoCasterSelectedEvent());
            return null;
        }

        var caster = world.Find(casterId.Value);
        if (caster == null)
        {
            eventTracker.Add(new CasterNotFoundEvent());
            return null;
        }

        if (checkForDeath && caster.Life == null)
        {
            eventTracker.Add(new CasterDeadEvent());
            return null;
        }

        return caster;
    }
}
