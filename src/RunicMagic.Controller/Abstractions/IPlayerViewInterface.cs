using RunicMagic.Controller.Models;

namespace RunicMagic.Controller.Abstractions;

public interface IPlayerViewInterface
{
    Task<CommandResult> RegisterInput(string input);

    Task<CommandResult> SetCaster(WorldCoordinate worldCoordinate);

    Task<CommandResult> MoveCaster(WorldCoordinate worldCoordinate);

    Task<CommandResult> SetPointingDirection(WorldCoordinate worldCoordinate);

    Task<CommandResult> SetIndicateTarget(WorldCoordinate worldCoordinate);

    string Prompt { get; }
}
