using RunicMagic.Controller.Services;
using RunicMagic.World.Entities;

namespace RunicMagic.Controller.Abstractions;

internal interface IPlayerGameLoopInterface
{
    EntityId? GetCasterId();

    void DrainAndFlush(EventTracker eventTracker);
}