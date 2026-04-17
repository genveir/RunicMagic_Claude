namespace RunicMagic.Controller.RuneParsing;

public abstract record ParseEvent;

public record RanOutOfTokensEvent : ParseEvent;

public record UnexpectedTokenEvent(string Token, string ExpectedType) : ParseEvent;
