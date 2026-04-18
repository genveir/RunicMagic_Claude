namespace RunicMagic.World;

public class TeleportEntityService
{
    public void Teleport(Entity entity, int x, int y)
    {
        entity.X = x;
        entity.Y = y;
    }
}
