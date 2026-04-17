using RunicMagic.Controller.RuneParsing;
using RunicMagic.World.Execution;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Tests.RuneParsing;

internal class MockEntitySet : IEntitySet
{
    public EntitySet Resolve(SpellContext context) => throw new NotImplementedException();
}

internal class MockNumber : INumber
{
    public Number Evaluate(SpellContext context) => throw new NotImplementedException();
}

internal class MockLocation : ILocation
{
    public Location Evaluate(SpellContext context) => throw new NotImplementedException();
}

internal class MockStatement : IStatement
{
    public void Execute(SpellContext context) => throw new NotImplementedException();
}

internal class MockParser<T> : IRuneParser<T>
{
    private readonly T _value;

    internal MockParser(T value)
    {
        _value = value;
    }

    public ParsingResult<T> Parse(TokenStream tokenStream)
    {
        return ParsingResult<T>.Succeed(_value);
    }
}

internal class MockFailingParser<T> : IRuneParser<T>
{
    private readonly ParseEvent _error;

    internal MockFailingParser(ParseEvent error)
    {
        _error = error;
    }

    public ParsingResult<T> Parse(TokenStream tokenStream)
    {
        return ParsingResult<T>.Fail(_error);
    }
}
