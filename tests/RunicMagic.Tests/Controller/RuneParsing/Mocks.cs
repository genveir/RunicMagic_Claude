using RunicMagic.Controller.RuneParsing;
using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Tests.Controller.RuneParsing;

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
    private readonly T value;

    internal MockParser(T value)
    {
        this.value = value;
    }

    public ParsingResult<T> Parse(TokenStream tokenStream)
    {
        return ParsingResult<T>.Succeed(value);
    }
}

internal class MockFailingParser<T> : IRuneParser<T>
{
    private readonly ParseEvent error;

    internal MockFailingParser(ParseEvent error)
    {
        this.error = error;
    }

    public ParsingResult<T> Parse(TokenStream tokenStream)
    {
        return ParsingResult<T>.Fail(error);
    }
}
