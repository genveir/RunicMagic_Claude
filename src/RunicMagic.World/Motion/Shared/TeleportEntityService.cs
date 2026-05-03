using RunicMagic.World.Entities;
using RunicMagic.World.Geometry;

namespace RunicMagic.World.Motion.Shared;

public static class TeleportEntityService
{
    public static void Teleport(Entity entity, Location location)
    {
        entity.Location = location;
    }
}
