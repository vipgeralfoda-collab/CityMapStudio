namespace CityMapStudio.Domain.Models
{
    /// <summary>
    /// Resoluções de preview disponíveis
    /// </summary>
    public enum PreviewResolution
    {
        R256 = 256,
        R512 = 512,
        R1024 = 1024
    }

    /// <summary>
    /// Representa um retângulo para operações de dirty region
    /// </summary>
    public struct RectI
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public RectI(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Clamp para um tamanho máximo (ex: heightmap size)
        /// </summary>
        public RectI Clamp(int maxWidth, int maxHeight)
        {
            int x = Math.Max(0, Math.Min(X, maxWidth - 1));
            int y = Math.Max(0, Math.Min(Y, maxHeight - 1));
            int w = Math.Min(Width, maxWidth - x);
            int h = Math.Min(Height, maxHeight - y);
            return new RectI(x, y, w, h);
        }

        /// <summary>
        /// Expande o retângulo em todas as direções por um valor
        /// </summary>
        public RectI Expand(int amount)
        {
            return new RectI(X - amount, Y - amount, Width + amount * 2, Height + amount * 2);
        }
    }
}
