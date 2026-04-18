using RunicMagic.Controller.Models;
using RunicMagic.World;

namespace RunicMagic.Controller.Mappers;

public static class EntityRenderingMapper
{
    public static EntityRenderingModel ToRenderingModel(Entity entity, bool isCaster, (long X, long Y)? pointingEnd = null, bool isIndicateTarget = false, (long X, long Y)? indicateEnd = null)
    {
        var flags = EntityRenderingFlags.None;
        if (entity.Life != null) flags |= EntityRenderingFlags.HasLife;
        if (entity.HasAgency) flags |= EntityRenderingFlags.HasAgency;
        if (entity.IsTranslucent) flags |= EntityRenderingFlags.IsTranslucent;

        return new EntityRenderingModel(
            X: entity.X,
            Y: entity.Y,
            Width: entity.Width,
            Height: entity.Height,
            Label: entity.Label,
            Flags: flags,
            IsCaster: isCaster,
            PointingEndX: pointingEnd?.X,
            PointingEndY: pointingEnd?.Y,
            IsIndicateTarget: isIndicateTarget,
            IndicateEndX: indicateEnd?.X,
            IndicateEndY: indicateEnd?.Y);
    }
}
