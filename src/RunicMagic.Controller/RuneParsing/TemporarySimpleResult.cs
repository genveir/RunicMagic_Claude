namespace RunicMagic.Controller.RuneParsing
{
    internal class TemporarySimpleResult<T>
    {
        private TemporarySimpleResult(bool succeeded, T? value, string errorMessage)
        {
            Succeeded = succeeded;
            _value = value;
            _errorMessage = errorMessage;
        }

        public static TemporarySimpleResult<T> Succeed(T value)
        {
            return new TemporarySimpleResult<T>(true, value, string.Empty);
        }

        public static TemporarySimpleResult<T> Fail(string errorMessage)
        {
            return new TemporarySimpleResult<T>(false, default, errorMessage);
        }

        public bool Succeeded { get; }

        private T? _value;
        public T Value
        {
            get => Succeeded ? _value! : throw new InvalidOperationException("Cannot access value when operation failed.");
            init => _value = value;
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => !Succeeded ? _errorMessage : throw new InvalidOperationException("No error message when operation succeeded.");
            init => _errorMessage = value;
        }
    }
}
