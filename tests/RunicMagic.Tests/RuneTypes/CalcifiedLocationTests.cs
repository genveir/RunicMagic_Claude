using FluentAssertions;
using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;
using RunicMagic.World.Runes.RuneTypes;
using RunicMagic.Tests.Execution;
using Xunit;

namespace RunicMagic.Tests.RuneTypes;

public class CalcifiedLocationTests
{
    private class CountingLocation : ILocation
    {
        public int EvaluateCallCount { get; private set; }
        private readonly Location _result;

        public CountingLocation(Location result)
        {
            _result = result;
        }

        public Location Evaluate(SpellContext context)
        {
            EvaluateCallCount++;
            return _result;
        }
    }

    [Fact]
    public void Evaluate_CalledOnce_DelegatesToInner()
    {
        var expectedLocation = new Location(100, 200);
        var inner = new CountingLocation(expectedLocation);
        var calcified = new CalcifiedLocation(inner);
        var context = TestFixtures.MakeContext();

        var result = calcified.Evaluate(context);

        result.Should().Be(expectedLocation);
    }

    [Fact]
    public void Evaluate_CalledTwice_OnlyCallsInnerOnce()
    {
        var inner = new CountingLocation(new Location(0, 0));
        var calcified = new CalcifiedLocation(inner);
        var context = TestFixtures.MakeContext();

        calcified.Evaluate(context);
        calcified.Evaluate(context);

        inner.EvaluateCallCount.Should().Be(1);
    }

    [Fact]
    public void Evaluate_CalledTwice_ReturnsSameCachedValue()
    {
        var expectedLocation = new Location(42, 99);
        var inner = new CountingLocation(expectedLocation);
        var calcified = new CalcifiedLocation(inner);
        var context = TestFixtures.MakeContext();

        var first = calcified.Evaluate(context);
        var second = calcified.Evaluate(context);

        second.Should().Be(first);
        second.Should().Be(expectedLocation);
    }

    [Fact]
    public void Inner_ReturnsWrappedLocation()
    {
        var inner = new CountingLocation(new Location(0, 0));
        var calcified = new CalcifiedLocation(inner);

        calcified.Inner.Should().BeSameAs(inner);
    }
}
