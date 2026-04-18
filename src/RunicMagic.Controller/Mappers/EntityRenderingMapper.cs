using RunicMagic.Controller.Models;
using RunicMagic.World;

namespace RunicMagic.Controller.Mappers;

public static class EntityRenderingMapper
{
    public static EntityRenderingModel ToRenderingModel(Entity entity, bool isCaster)
    {
        var flags = EntityRenderingFlags.None;
        if (entity.Life != null) flags |= EntityRenderingFlags.HasLife;
        if (entity.HasAgency) flags |= EntityRenderingFlags.HasAgency;

        return new EntityRenderingModel(
            X: entity.X,
            Y: entity.Y,
            Width: entity.Width,
            Height: entity.Height,
            Label: entity.Label,
            Flags: flags,
            IsCaster: isCaster);
    }
}
