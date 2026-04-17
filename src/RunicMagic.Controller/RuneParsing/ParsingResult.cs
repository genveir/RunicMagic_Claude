namespace RunicMagic.Controller.RuneParsing
{
    internal class ParsingResult<T>
    {
        private ParsingResult(bool succeeded, T? value, ParseEvent? error)
        {
            Succeeded = succeeded;
            _value = value;
            _error = error;
        }

        public static ParsingResult<T> Succeed(T value)
        {
            return new ParsingResult<T>(true, value, null);
        }

        public static ParsingResult<T> Fail(ParseEvent error)
        {
            return new ParsingResult<T>(false, default, error);
        }

        public bool Succeeded { get; }

        private T? _value;
        public T Value
        {
            get => Succeeded ? _value! : throw new InvalidOperationException("Cannot access value when operation failed.");
            init => _value = value;
        }

        private ParseEvent? _error;
        public ParseEvent Error
        {
            get => !Succeeded ? _error! : throw new InvalidOperationException("No error when operation succeeded.");
            init => _error = value;
        }
    }
}
