using RunicMagic.World.Execution;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Tests.RuneTypes;

public class CalcifiedNumberTests
{
    private class CountingNumber : INumber
    {
        public int EvaluateCallCount { get; private set; }
        private readonly Number _result;

        public CountingNumber(Number result)
        {
            _result = result;
        }

        public Number Evaluate(SpellContext context)
        {
            EvaluateCallCount++;
            return _result;
        }
    }

    [Fact]
    public void Evaluate_CalledOnce_DelegatesToInner()
    {
        var expectedNumber = new Number(42);
        var inner = new CountingNumber(expectedNumber);
        var calcified = new CalcifiedNumber(inner);
        var context = TestFixtures.MakeContext();

        var result = calcified.Evaluate(context);

        result.Should().Be(expectedNumber);
    }

    [Fact]
    public void Evaluate_CalledTwice_OnlyCallsInnerOnce()
    {
        var inner = new CountingNumber(new Number(0));
        var calcified = new CalcifiedNumber(inner);
        var context = TestFixtures.MakeContext();

        calcified.Evaluate(context);
        calcified.Evaluate(context);

        inner.EvaluateCallCount.Should().Be(1);
    }

    [Fact]
    public void Evaluate_CalledTwice_ReturnsSameCachedValue()
    {
        var expectedNumber = new Number(999);
        var inner = new CountingNumber(expectedNumber);
        var calcified = new CalcifiedNumber(inner);
        var context = TestFixtures.MakeContext();

        var first = calcified.Evaluate(context);
        var second = calcified.Evaluate(context);

        second.Should().Be(first);
        second.Should().Be(expectedNumber);
    }

    [Fact]
    public void Inner_ReturnsWrappedNumber()
    {
        var inner = new CountingNumber(new Number(0));
        var calcified = new CalcifiedNumber(inner);

        calcified.Inner.Should().BeSameAs(inner);
    }
}
