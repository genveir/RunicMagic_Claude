namespace RunicMagic.Controller.Models
{
    [Flags]
    public enum EntityRenderingFlags { None = 0, HasLife = 1, HasAgency = 2 }

    public class EntityRenderingModel
    {
        public int X { get; }
        public int Y { get; }

        public int Width { get; }
        public int Height { get; }

        public string Label { get; }

        public EntityRenderingFlags Flags { get; }

        public EntityRenderingModel(int x, int y, int width, int height, string label, EntityRenderingFlags flags)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Label = label;
            Flags = flags;
        }
    }
}
