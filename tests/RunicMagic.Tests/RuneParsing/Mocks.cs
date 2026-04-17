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

    public TemporarySimpleResult<T> Parse(TokenStream tokenStream)
    {
        return TemporarySimpleResult<T>.Succeed(_value);
    }
}

internal class MockFailingParser<T> : IRuneParser<T>
{
    private readonly string _errorMessage;

    internal MockFailingParser(string errorMessage)
    {
        _errorMessage = errorMessage;
    }

    public TemporarySimpleResult<T> Parse(TokenStream tokenStream)
    {
        return TemporarySimpleResult<T>.Fail(_errorMessage);
    }
}
