using RunicMagic.Controller.Models;

namespace RunicMagic.Controller.Abstractions;

public interface IPlayerViewInterface
{
    Task<CommandResult> RegisterInput(string input);
    string Prompt { get; }
}
