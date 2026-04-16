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
            x: entity.Bounds.X,
            y: entity.Bounds.Y,
            width: entity.Bounds.Width,
            height: entity.Bounds.Height,
            label: entity.Label,
            flags: flags);
    }
}
