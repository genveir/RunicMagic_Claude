namespace RunicMagic.Controller.RuneParsing
{
    internal static class RuneParsingDispatcher
    {
        internal static TemporarySimpleResult<TRuneType> ParseNextRune<TRuneType>(TokenStream tokenStream)
        {
            var next = tokenStream.Next();
            if (next == null)
            {
                return TemporarySimpleResult<TRuneType>.Fail("This should be a typed result signifying a spell ran out of tokens and is incomplete");
            }

            return ParseCurrentRune<TRuneType>(tokenStream, next!);
        }

        internal static TemporarySimpleResult<TRuneType> ParseNextRune<TRuneType>(TokenStream tokenStream, string[] defaultTokens)
        {
            if (defaultTokens.Length == 0)
            {
                throw new InvalidOperationException("Cannot provide an empty array of default tokens.");
            }

            var next = tokenStream.Next();
            if (next == null)
            {
                tokenStream.InsertAtCursor(defaultTokens);
                return ParseCurrentRune<TRuneType>(tokenStream, defaultTokens[0]);
            }

            var runeTypeParser = ParserLookup.FindRuneParserByName<TRuneType>(next);
            if (runeTypeParser == null)
            {
                tokenStream.InsertAtCursor(defaultTokens);
                return ParseCurrentRune<TRuneType>(tokenStream, defaultTokens[0]);
            }

            return ParseCurrentRune<TRuneType>(tokenStream, next);
        }

        private static TemporarySimpleResult<TRuneType> ParseCurrentRune<TRuneType>(TokenStream tokenStream, string current)
        {
            var runeTypeParser = ParserLookup.FindRuneParserByName<TRuneType>(current);
            if (runeTypeParser == null)
            {
                return TemporarySimpleResult<TRuneType>.Fail("This should be a typed result signifying an invalid token was encountered");
            }

            var parseResult = runeTypeParser.Parse(tokenStream);
            return parseResult.Succeeded
                ? TemporarySimpleResult<TRuneType>.Succeed(parseResult.Value)
                : TemporarySimpleResult<TRuneType>.Fail(parseResult.ErrorMessage);
        }
    }
}
