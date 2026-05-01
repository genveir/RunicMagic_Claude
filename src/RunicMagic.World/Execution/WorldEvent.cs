namespace RunicMagic.World.Execution;

public abstract record WorldEvent;

public record EntityPushedEvent(Entity Entity, long DistanceMm) : WorldEvent;
public record EntityPulledEvent(Entity Entity, long DistanceMm) : WorldEvent;
public record PowerDrawnEvent(Entity Entity, long Amount) : WorldEvent;
public record EntityDrainedEvent(Entity Entity) : WorldEvent;
public record EntityDamagedEvent(Entity Entity, long Amount) : WorldEvent;
public record PowerFilledEvent(Entity Entity, long Amount) : WorldEvent;
public record EntityFullEvent(Entity Entity) : WorldEvent;
public record EffectNotFiredEvent(string Effect, string Reason) : WorldEvent;
public record EntityDisintegratedEvent(Entity Entity) : WorldEvent;
public record SelectionCostNotMetEvent(long Required, long Drawn) : WorldEvent;
public record InscriptionReadEvent(Entity Entity, string Text) : WorldEvent;
public record EntityRotatedEvent(Entity Entity, long AngleDegrees) : WorldEvent;

public record DebugOutputEvent(string Text) : WorldEvent;
