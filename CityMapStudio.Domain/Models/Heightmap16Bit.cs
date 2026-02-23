namespace CityMapStudio.Domain.Models
{
    public class Heightmap16Bit
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public ushort[] Data { get; set; }

        public Heightmap16Bit(int width, int height)
        {
            Width = width;
            Height = height;
            Data = new ushort[width * height];
        }

        public ushort GetHeight(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return 0;
            return Data[y * Width + x];
        }

        public void SetHeight(int x, int y, ushort value)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return;
            Data[y * Width + x] = value;
        }

        /// <summary>
        /// Cria uma versão redimensionada do heightmap (downsample)
        /// Usa interpolação bilinear para suavizar (mais rápido que box filter)
        /// Paralelizado para máxima performance
        /// </summary>
        public Heightmap16Bit Downsample(int targetWidth, int targetHeight)
        {
            if (targetWidth >= Width || targetHeight >= Height)
                return this;

            var downsampled = new Heightmap16Bit(targetWidth, targetHeight);

            float scaleX = (float)Width / targetWidth;
            float scaleY = (float)Height / targetHeight;

            // Paralelizar para máxima performance
            System.Threading.Tasks.Parallel.For(0, targetHeight, y =>
            {
                for (int x = 0; x < targetWidth; x++)
                {
                    // Interpolação bilinear
                    float srcX = x * scaleX;
                    float srcY = y * scaleY;

                    int x0 = (int)srcX;
                    int y0 = (int)srcY;
                    int x1 = Math.Min(x0 + 1, Width - 1);
                    int y1 = Math.Min(y0 + 1, Height - 1);

                    float fx = srcX - x0;
                    float fy = srcY - y0;

                    // Amostra dos 4 vizinhos
                    ushort v00 = GetHeight(x0, y0);
                    ushort v10 = GetHeight(x1, y0);
                    ushort v01 = GetHeight(x0, y1);
                    ushort v11 = GetHeight(x1, y1);

                    // Interpolação
                    float v0 = v00 * (1 - fx) + v10 * fx;
                    float v1 = v01 * (1 - fx) + v11 * fx;
                    float result = v0 * (1 - fy) + v1 * fy;

                    downsampled.SetHeight(x, y, (ushort)result);
                }
            });

            return downsampled;
        }

        /// <summary>
        /// Calcula estatísticas do heightmap
        /// </summary>
        public HeightmapStatistics GetStatistics()
        {
            if (Data.Length == 0)
                return new HeightmapStatistics(0, 0, 0, 0);

            ushort min = ushort.MaxValue;
            ushort max = ushort.MinValue;
            double sum = 0;

            foreach (var height in Data)
            {
                if (height < min) min = height;
                if (height > max) max = height;
                sum += height;
            }

            double avg = sum / Data.Length;

            // Calcular desvio padrão
            double sumSqDiff = 0;
            foreach (var height in Data)
            {
                double diff = height - avg;
                sumSqDiff += diff * diff;
            }

            double stdDev = Math.Sqrt(sumSqDiff / Data.Length);

            return new HeightmapStatistics(min, max, avg, stdDev);
        }

        /// <summary>
        /// Clone do heightmap
        /// </summary>
        public Heightmap16Bit Clone()
        {
            var clone = new Heightmap16Bit(Width, Height);
            Array.Copy(Data, clone.Data, Data.Length);
            return clone;
        }
    }
}

