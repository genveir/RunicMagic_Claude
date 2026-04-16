using RunicMagic.Controller.Models;
using RunicMagic.World;

namespace RunicMagic.Controller.Mappers;

public static class EntityRenderingMapper
{
    public static EntityRenderingModel ToRenderingModel(Entity entity)
    {
        var flags = EntityRenderingFlags.None;
        if (entity.Life != null) flags |= EntityRenderingFlags.HasLife;
        if (entity.HasAgency) flags |= EntityRenderingFlags.HasAgency;

        return new EntityRenderingModel(
            x: entity.X,
            y: entity.Y,
            width: entity.Width,
            height: entity.Height,
            label: entity.Label,
            flags: flags);
    }
}
