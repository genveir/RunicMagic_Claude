namespace RunicMagic.Controller.RuneParsing
{
    internal interface IRuneParser<T>
    {
        TemporarySimpleResult<T> Parse(TokenStream tokenStream);
    }
}
