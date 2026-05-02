using RunicMagic.World.Execution;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Tests.RuneTypes;

public class CalcifiedEntitySetTests
{
    private class CountingEntitySet : IEntitySet
    {
        public int ResolveCallCount { get; private set; }
        private readonly EntitySet _result;

        public CountingEntitySet(EntitySet result)
        {
            _result = result;
        }

        public EntitySet Resolve(SpellContext context)
        {
            ResolveCallCount++;
            return _result;
        }
    }

    [Fact]
    public void Resolve_CalledOnce_DelegatesToInner()
    {
        var entity = new EntityBuilder().Build();
        var expectedSet = new EntitySet([entity]);
        var inner = new CountingEntitySet(expectedSet);
        var calcified = new CalcifiedEntitySet(inner);
        var context = TestFixtures.MakeContext();

        var result = calcified.Resolve(context);

        result.Should().BeSameAs(expectedSet);
    }

    [Fact]
    public void Resolve_CalledTwice_OnlyCallsInnerOnce()
    {
        var inner = new CountingEntitySet(new EntitySet([]));
        var calcified = new CalcifiedEntitySet(inner);
        var context = TestFixtures.MakeContext();

        calcified.Resolve(context);
        calcified.Resolve(context);

        inner.ResolveCallCount.Should().Be(1);
    }

    [Fact]
    public void Resolve_CalledTwice_ReturnsSameCachedInstance()
    {
        var entity = new EntityBuilder().Build();
        var expectedSet = new EntitySet([entity]);
        var inner = new CountingEntitySet(expectedSet);
        var calcified = new CalcifiedEntitySet(inner);
        var context = TestFixtures.MakeContext();

        var first = calcified.Resolve(context);
        var second = calcified.Resolve(context);

        second.Should().BeSameAs(first);
    }

    [Fact]
    public void Inner_ReturnsWrappedEntitySet()
    {
        var inner = new CountingEntitySet(new EntitySet([]));
        var calcified = new CalcifiedEntitySet(inner);

        calcified.Inner.Should().BeSameAs(inner);
    }
}
