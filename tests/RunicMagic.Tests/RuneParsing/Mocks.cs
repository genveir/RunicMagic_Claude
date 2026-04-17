using RunicMagic.Controller.RuneParsing;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Tests.RuneParsing;

internal class MockEntitySet : IEntitySet { }
internal class MockNumber : INumber { }
internal class MockLocation : ILocation { }
internal class MockStatement : IStatement { }

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
