using RunicMagic.Players.Models;

namespace RunicMagic.Players.Abstractions;

public interface IPlayerViewInterface
{
    void SetDataHandlers(Func<string, Task> onTextDataAvailable, Func<EntityRenderingModel, Task> onRenderingDataAvailable, Func<Task> onTickComplete);

    Task RegisterInput(string input);

    string Prompt { get; }
}
