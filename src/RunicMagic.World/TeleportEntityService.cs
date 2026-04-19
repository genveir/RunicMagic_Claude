using RunicMagic.World.Geometry;

namespace RunicMagic.World;

public class TeleportEntityService
{
    public void Teleport(Entity entity, Location location)
    {
        entity.Location = location;
    }
}
