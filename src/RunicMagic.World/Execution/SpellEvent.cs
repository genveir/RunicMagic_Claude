namespace RunicMagic.World.Execution;

public abstract record SpellEvent;

public record EntityPushedEvent(Entity Entity, int DistanceMm) : SpellEvent;
public record EntityPulledEvent(Entity Entity, int DistanceMm) : SpellEvent;
public record PowerDrawnEvent(Entity Entity, int Amount) : SpellEvent;
public record EntityDrainedEvent(Entity Entity) : SpellEvent;
public record EffectNotFiredEvent(string Effect, string Reason) : SpellEvent;
public record ExecutorDisintegratedEvent : SpellEvent;
