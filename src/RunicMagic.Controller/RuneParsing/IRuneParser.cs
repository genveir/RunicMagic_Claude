namespace RunicMagic.Controller.RuneParsing
{
    internal interface IRuneParser<T>
    {
        ParsingResult<T> Parse(TokenStream tokenStream);
    }
}
