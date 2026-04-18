namespace RunicMagic.Controller.Models
{
    [Flags]
    public enum EntityRenderingFlags { None = 0, HasLife = 1, HasAgency = 2 }

    public record EntityRenderingModel(int X, int Y, int Width, int Height, string Label, EntityRenderingFlags Flags, bool IsCaster);
}
