namespace RunicMagic.Controller.Models
{
    [Flags]
    public enum EntityRenderingFlags { None = 0, HasLife = 1, HasAgency = 2, IsTranslucent = 4 }

    public record EntityRenderingModel(double X, double Y, double Width, double Height, double Angle, string Label, EntityRenderingFlags Flags, bool IsCaster, double? PointingEndX, double? PointingEndY, bool IsIndicateTarget, double? IndicateEndX, double? IndicateEndY);
}
