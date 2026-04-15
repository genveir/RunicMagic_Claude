using RunicMagic.Players.Models;

namespace RunicMagic.Players.Abstractions;

public interface IPlayerViewInterface
{
    Task<CommandResult> RegisterInput(string input);
    string Prompt { get; }
}
