using RunicMagic.World.Execution;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Controller.RuneParsing;

internal static class RuneParsingDispatcher
{
    internal static ParsingResult<IEntitySet> ParseNextTaxedEntitySet(TokenStream tokenStream)
    {
        var result = DispatchNextRune<IEntitySet>(tokenStream);
        if (!result.Succeeded)
        {
            return result;
        }
        return WrapTaxedEntitySet(tokenStream, result);
    }

    internal static ParsingResult<IEntitySet> ParseNextTaxedEntitySet(TokenStream tokenStream, string[] defaultTokens)
    {
        var result = DispatchNextRune<IEntitySet>(tokenStream, defaultTokens);
        if (!result.Succeeded)
        {
            return result;
        }
        return WrapTaxedEntitySet(tokenStream, result);
    }

    internal static ParsingResult<TRuneType> ParseNextRune<TRuneType>(TokenStream tokenStream)
    {
        var result = DispatchNextRune<TRuneType>(tokenStream);
        return WrapIfCalcified(tokenStream, result);
    }

    internal static ParsingResult<TRuneType> ParseNextRune<TRuneType>(TokenStream tokenStream, string[] defaultTokens)
    {
        var result = DispatchNextRune<TRuneType>(tokenStream, defaultTokens);
        return WrapIfCalcified(tokenStream, result);
    }

    private static ParsingResult<IEntitySet> WrapTaxedEntitySet(TokenStream tokenStream, ParsingResult<IEntitySet> result)
    {
        var costResolved = new EntitySetSelectionCostResolver(result.Value);
        if (tokenStream.LivenessMode == LivenessMode.Calcified && !result.IsLive)
        {
            return ParsingResult<IEntitySet>.Succeed(new CalcifiedEntitySet(costResolved), isLive: false);
        }
        return ParsingResult<IEntitySet>.Succeed(costResolved, isLive: result.IsLive);
    }

    private static ParsingResult<TRuneType> WrapIfCalcified<TRuneType>(TokenStream tokenStream, ParsingResult<TRuneType> result)
    {
        if (!result.Succeeded)
        {
            return result;
        }
        if (tokenStream.LivenessMode != LivenessMode.Calcified || result.IsLive)
        {
            return result;
        }

        if (result.Value is IEntitySet es && es is not CalcifiedEntitySet)
        {
            return ParsingResult<TRuneType>.Succeed((TRuneType)(object)new CalcifiedEntitySet(es), isLive: false);
        }
        if (result.Value is ILocation loc && loc is not CalcifiedLocation)
        {
            return ParsingResult<TRuneType>.Succeed((TRuneType)(object)new CalcifiedLocation(loc), isLive: false);
        }
        if (result.Value is INumber num && num is not CalcifiedNumber)
        {
            return ParsingResult<TRuneType>.Succeed((TRuneType)(object)new CalcifiedNumber(num), isLive: false);
        }

        return result;
    }

    private static ParsingResult<TRuneType> DispatchNextRune<TRuneType>(TokenStream tokenStream, string[]? defaultTokens = null)
    {
        if (defaultTokens == null)
        {
            var next = tokenStream.Next();
            if (next == null)
            {
                return ParsingResult<TRuneType>.Fail(new RanOutOfTokensEvent());
            }
            return ParseCurrentRune<TRuneType>(tokenStream, next);
        }

        if (defaultTokens.Length == 0)
        {
            throw new InvalidOperationException("Cannot provide an empty array of default tokens.");
        }

        var token = tokenStream.Next();
        if (token == null)
        {
            tokenStream.InsertAtCursor(defaultTokens);
            var inserted = tokenStream.Next()!;
            return ParseCurrentRune<TRuneType>(tokenStream, inserted);
        }

        var runeTypeParser = ParserLookup.FindRuneParserByName<TRuneType>(token);
        if (runeTypeParser == null)
        {
            tokenStream.Backtrack();
            tokenStream.InsertAtCursor(defaultTokens);
            var inserted = tokenStream.Next()!;
            return ParseCurrentRune<TRuneType>(tokenStream, inserted);
        }

        return ParseCurrentRune<TRuneType>(tokenStream, token);
    }

    private static ParsingResult<TRuneType> ParseCurrentRune<TRuneType>(TokenStream tokenStream, string current)
    {
        var runeTypeParser = ParserLookup.FindRuneParserByName<TRuneType>(current);
        if (runeTypeParser == null)
        {
            return ParsingResult<TRuneType>.Fail(new UnexpectedTokenEvent(current, typeof(TRuneType).Name));
        }

        var parseResult = runeTypeParser.Parse(tokenStream);
        return parseResult.Succeeded
            ? ParsingResult<TRuneType>.Succeed(parseResult.Value, parseResult.IsLive)
            : ParsingResult<TRuneType>.Fail(parseResult.Error);
    }
}
