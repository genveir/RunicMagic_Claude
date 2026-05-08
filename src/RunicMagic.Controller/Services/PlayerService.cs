using RunicMagic.Controller.Abstractions;
using RunicMagic.Controller.Models;
using RunicMagic.View.Abstractions;
using RunicMagic.View.Models;
using RunicMagic.World;
using RunicMagic.World.Entities;
using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;
using RunicMagic.World.Motion.Shared;
using System.Collections.Concurrent;

namespace RunicMagic.Controller.Services;

internal class PlayerService : IPlayerViewInterface, IPlayerGameLoopInterface
{
    private readonly ConcurrentQueue<Action<EventTracker>> queue;
    private readonly WorldModel world;
    private readonly SpellCastingService spellCasting;
    private readonly RayCastService rayCast;

    private EntityId? _casterId = null;

    public PlayerService(
            WorldModel world,
            SpellCastingService spellCasting,
            RayCastService rayCast)
    {
        queue = new();
        this.world = world;
        this.spellCasting = spellCasting;
        this.rayCast = rayCast;
    }

    public Task RegisterInput(string input)
    {
        queue.Enqueue((eventTracker) =>
        {
            var caster = CheckForCaster(eventTracker);
            if (caster == null) return;

            spellCasting.Cast(input, caster, eventTracker);
        });
        return Task.CompletedTask;
    }

    public Task SetCaster(WorldCoordinate worldCoordinate)
    {
        queue.Enqueue((eventTracker) =>
        {
            var entities = world.GetEntitiesAtPoint(new Location(worldCoordinate.X, worldCoordinate.Y))
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
                _casterId = casterEntity.Id;
                eventTracker.Add(new CasterSetEvent(casterEntity));
            }
        });
        return Task.CompletedTask;
    }

    public Task MoveCaster(WorldCoordinate worldCoordinate)
    {
        queue.Enqueue((eventTracker) =>
        {
            var caster = CheckForCaster(eventTracker);
            if (caster == null) return;

            TeleportEntityService.Teleport(caster, new Location(worldCoordinate.X, worldCoordinate.Y));
            eventTracker.Add(new CasterMovedEvent());
        });
        return Task.CompletedTask;
    }

    public Task SetPointingDirection(WorldCoordinate worldCoordinate)
    {
        queue.Enqueue((eventTracker) =>
        {
            var caster = CheckForCaster(eventTracker);
            if (caster == null) return;

            var to = new Location(worldCoordinate.X, worldCoordinate.Y);
            caster.PointingDirection = Direction.FromPoints(caster.Location, to);
            eventTracker.Add(new PointingDirectionSetEvent());
        });
        return Task.CompletedTask;
    }

    public Task SetIndicateTarget(WorldCoordinate worldCoordinate)
    {
        queue.Enqueue((eventTracker) =>
        {
            var caster = CheckForCaster(eventTracker);
            if (caster == null) return;

            var entities = world.GetEntitiesAtPoint(new Location(worldCoordinate.X, worldCoordinate.Y));
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

            var to = new Location(worldCoordinate.X, worldCoordinate.Y);
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

    public EntityId? GetCasterId()
    {
        return _casterId;
    }

    public void DrainAndFlush(EventTracker eventTracker)
    {
        while (queue.TryDequeue(out var action))
            action(eventTracker);
    }

    private Entity? CheckForCaster(EventTracker eventTracker)
    {
        if (_casterId == null)
        {
            eventTracker.Add(new NoCasterSelectedEvent());
            return null;
        }

        var caster = world.Find(_casterId.Value);
        if (caster == null)
        {
            eventTracker.Add(new CasterNotFoundEvent());
            return null;
        }

        if (caster.StructuralIntegrity.CurrentIntegrity <= 0)
        {
            eventTracker.Add(new CasterDestroyedEvent());
            return null;
        }

        if (caster.Life != null && caster.Life.CurrentHitPoints <= 0)
        {
            eventTracker.Add(new CasterDeadEvent());
            return null;
        }

        return caster;
    }
}
