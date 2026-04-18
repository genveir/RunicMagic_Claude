namespace RunicMagic.Controller.Models
{
    [Flags]
    public enum EntityRenderingFlags { None = 0, HasLife = 1, HasAgency = 2, IsTranslucent = 4 }

    public record EntityRenderingModel(int X, int Y, int Width, int Height, string Label, EntityRenderingFlags Flags, bool IsCaster, int? PointingEndX, int? PointingEndY, bool IsIndicateTarget, int? IndicateEndX, int? IndicateEndY);
}
