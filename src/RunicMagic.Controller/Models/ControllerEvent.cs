using RunicMagic.World.Entities;

namespace RunicMagic.Controller.Models;

public abstract record ControllerEvent;

public record NoCasterSelectedEvent : ControllerEvent;
public record CasterNotFoundEvent : ControllerEvent;
public record CasterDeadEvent : ControllerEvent;

public record NoEntitiesWithAgencyFoundEvent : ControllerEvent;
public record MultipleEntitiesWithAgencyFoundEvent : ControllerEvent;
public record CasterSetEvent(Entity Entity) : ControllerEvent;

public record CasterMovedEvent : ControllerEvent;

public record PointingDirectionSetEvent : ControllerEvent;

public record NothingToIndicateEvent : ControllerEvent;
public record IndicatingEvent(Entity Entity) : ControllerEvent;
public record IndicateTargetBlockedEvent : ControllerEvent;
public record IndicateTargetOutOfReachEvent(Entity Entity) : ControllerEvent;
