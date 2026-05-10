using RunicMagic.World.Entities.AI.Behaviors;

namespace RunicMagic.World.Entities.AI;

public class AICapability
{
    private readonly List<IAIBehavior> behaviors;

    public int BehaviorCount => behaviors.Count;

    public AICapability(List<IAIBehavior> behaviors)
    {
        this.behaviors = behaviors;
    }

    public void AddBehavior(IAIBehavior behavior)
    {
        behaviors.Add(behavior);
    }

    public void RemoveBehavior(IAIBehavior behavior)
    {
        behaviors.Remove(behavior);
    }

    public void RemoveBehavior(Func<IAIBehavior, bool> predicate)
    {
        behaviors.RemoveAll(b => predicate(b));
    }

    public void Execute(Entity entity, WorldModel worldModel, IWorldEventTracker eventTracker, long currentTick)
    {
        foreach (var behavior in behaviors)
        {
            behavior.Execute(entity, worldModel, eventTracker, currentTick);
        }
    }
}
