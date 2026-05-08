using RunicMagic.Controller.Services;
using RunicMagic.World;
using RunicMagic.World.Entities;
using RunicMagic.World.Entities.AI;

namespace RunicMagic.Tests.World.Entities.AI;

public class AICapabilityTests
{
    // ── BehaviorCount ─────────────────────────────────────────────────────────

    [Fact]
    public void BehaviorCount_IsZero_WhenConstructedEmpty()
    {
        var ai = new AICapability([]);

        ai.BehaviorCount.Should().Be(0);
    }

    // ── AddBehavior ───────────────────────────────────────────────────────────

    [Fact]
    public void AddBehavior_IncreasesBehaviorCount()
    {
        var ai = new AICapability([]);

        ai.AddBehavior(new RecordingBehavior());

        ai.BehaviorCount.Should().Be(1);
    }

    // ── RemoveBehavior ────────────────────────────────────────────────────────

    [Fact]
    public void RemoveBehavior_ByInstance_DecreasesBehaviorCount()
    {
        var behavior = new RecordingBehavior();
        var ai = new AICapability([]);
        ai.AddBehavior(behavior);

        ai.RemoveBehavior(behavior);

        ai.BehaviorCount.Should().Be(0);
    }

    [Fact]
    public void RemoveBehavior_ByPredicate_RemovesMatchingBehaviors()
    {
        var toRemove = new TaggedBehavior("remove");
        var toKeep = new TaggedBehavior("keep");
        var ai = new AICapability([]);
        ai.AddBehavior(toRemove);
        ai.AddBehavior(toKeep);

        ai.RemoveBehavior(b => b is TaggedBehavior t && t.Tag == "remove");

        ai.BehaviorCount.Should().Be(1);
    }

    // ── Execute ───────────────────────────────────────────────────────────────

    [Fact]
    public void Execute_CallsExecuteOnEachBehavior()
    {
        var b1 = new RecordingBehavior();
        var b2 = new RecordingBehavior();
        var ai = new AICapability([]);
        ai.AddBehavior(b1);
        ai.AddBehavior(b2);

        var entity = new EntityBuilder().Build();
        var world = new WorldModelBuilder().Build();
        var tracker = new EventTracker();

        ai.Execute(entity, world, tracker, currentTick: 0);

        b1.CallCount.Should().Be(1);
        b2.CallCount.Should().Be(1);
    }

    [Fact]
    public void Execute_DoesNotCallRemovedBehavior()
    {
        var behavior = new RecordingBehavior();
        var ai = new AICapability([]);
        ai.AddBehavior(behavior);
        ai.RemoveBehavior(behavior);

        var entity = new EntityBuilder().Build();
        var world = new WorldModelBuilder().Build();
        var tracker = new EventTracker();

        ai.Execute(entity, world, tracker, currentTick: 0);

        behavior.CallCount.Should().Be(0);
    }

    [Fact]
    public void Execute_PassesCorrectArgumentsToBehavior()
    {
        var behavior = new RecordingBehavior();
        var ai = new AICapability([]);
        ai.AddBehavior(behavior);

        var entity = new EntityBuilder().Build();
        var world = new WorldModelBuilder().Build();
        var tracker = new EventTracker();

        ai.Execute(entity, world, tracker, currentTick: 42);

        behavior.ReceivedEntity.Should().BeSameAs(entity);
        behavior.ReceivedWorld.Should().BeSameAs(world);
        behavior.ReceivedTracker.Should().BeSameAs(tracker);
        behavior.ReceivedTick.Should().Be(42);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private class RecordingBehavior : IAIBehavior
    {
        public int CallCount { get; private set; }
        public Entity? ReceivedEntity { get; private set; }
        public WorldModel? ReceivedWorld { get; private set; }
        public IWorldEventTracker? ReceivedTracker { get; private set; }
        public long ReceivedTick { get; private set; }

        public void Execute(Entity entity, WorldModel worldModel, IWorldEventTracker eventTracker, long currentTick)
        {
            CallCount++;
            ReceivedEntity = entity;
            ReceivedWorld = worldModel;
            ReceivedTracker = eventTracker;
            ReceivedTick = currentTick;
        }
    }

    private class TaggedBehavior : IAIBehavior
    {
        public string Tag { get; }

        public TaggedBehavior(string tag)
        {
            Tag = tag;
        }

        public void Execute(Entity entity, WorldModel worldModel, IWorldEventTracker eventTracker, long currentTick) { }
    }
}
