namespace RunicMagic.World;

public class TeleportEntityService
{
    public void Teleport(Entity entity, long x, long y)
    {
        entity.X = x;
        entity.Y = y;
    }
}
