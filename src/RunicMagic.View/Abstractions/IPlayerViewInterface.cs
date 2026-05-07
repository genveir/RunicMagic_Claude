using RunicMagic.View.Models;

namespace RunicMagic.View.Abstractions;

public interface IPlayerViewInterface
{
    Task RegisterInput(string input);

    Task SetCaster(WorldCoordinate worldCoordinate);

    Task MoveCaster(WorldCoordinate worldCoordinate);

    Task SetPointingDirection(WorldCoordinate worldCoordinate);

    Task SetIndicateTarget(WorldCoordinate worldCoordinate);
}
