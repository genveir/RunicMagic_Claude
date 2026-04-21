namespace RunicMagic.World.Execution;

public abstract record SpellEvent;

public record EntityPushedEvent(Entity Entity, long DistanceMm) : SpellEvent;
public record EntityPulledEvent(Entity Entity, long DistanceMm) : SpellEvent;
public record PowerDrawnEvent(Entity Entity, long Amount) : SpellEvent;
public record EntityDrainedEvent(Entity Entity) : SpellEvent;
public record EffectNotFiredEvent(string Effect, string Reason) : SpellEvent;
public record ExecutorDisintegratedEvent : SpellEvent;
public record SelectionCostNotMetEvent(long Required, long Drawn) : SpellEvent;
public record InscriptionReadEvent(Entity Entity, string Text) : SpellEvent;
public record EntityRotatedEvent(Entity Entity, long AngleDegrees) : SpellEvent;
