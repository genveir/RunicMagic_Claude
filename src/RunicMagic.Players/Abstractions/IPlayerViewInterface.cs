namespace RunicMagic.Players.Abstractions;

public interface IPlayerViewInterface
{
    void SetDataHandlers(Func<string, Task> onDataAvailable, Func<Task> onTickComplete);

    Task RegisterInput(string input);

    string Prompt { get; }
}
