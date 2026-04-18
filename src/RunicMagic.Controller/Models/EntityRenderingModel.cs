namespace RunicMagic.Controller.Models
{
    [Flags]
    public enum EntityRenderingFlags { None = 0, HasLife = 1, HasAgency = 2, IsTranslucent = 4 }

    public record EntityRenderingModel(long X, long Y, long Width, long Height, string Label, EntityRenderingFlags Flags, bool IsCaster, long? PointingEndX, long? PointingEndY, bool IsIndicateTarget, long? IndicateEndX, long? IndicateEndY);
}
