namespace RunicMagic.Controller.RuneParsing
{
    internal static class RuneParsingDispatcher
    {
        internal static ParsingResult<TRuneType> ParseNextRune<TRuneType>(TokenStream tokenStream)
        {
            var next = tokenStream.Next();
            if (next == null)
            {
                return ParsingResult<TRuneType>.Fail(new RanOutOfTokensEvent());
            }

            return ParseCurrentRune<TRuneType>(tokenStream, next!);
        }

        internal static ParsingResult<TRuneType> ParseNextRune<TRuneType>(TokenStream tokenStream, string[] defaultTokens)
        {
            if (defaultTokens.Length == 0)
            {
                throw new InvalidOperationException("Cannot provide an empty array of default tokens.");
            }

            var next = tokenStream.Next();
            if (next == null)
            {
                tokenStream.InsertAtCursor(defaultTokens);
                next = tokenStream.Next()!;
                return ParseCurrentRune<TRuneType>(tokenStream, next);
            }

            var runeTypeParser = ParserLookup.FindRuneParserByName<TRuneType>(next);
            if (runeTypeParser == null)
            {
                tokenStream.Backtrack();
                tokenStream.InsertAtCursor(defaultTokens);
                next = tokenStream.Next()!;
                return ParseCurrentRune<TRuneType>(tokenStream, next);
            }

            return ParseCurrentRune<TRuneType>(tokenStream, next);
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
                ? ParsingResult<TRuneType>.Succeed(parseResult.Value)
                : ParsingResult<TRuneType>.Fail(parseResult.Error);
        }
    }
}
