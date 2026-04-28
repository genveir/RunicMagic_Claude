using RunicMagic.World.Geometry;

namespace RunicMagic.World.Services;

public static class TeleportEntityService
{
    public static void Teleport(Entity entity, Location location)
    {
        entity.Location = location;
    }
}
