using CityMapStudio.Domain.Models;

namespace CityMapStudio.Application.Services
{
    public interface IHeightmapSmoothingService
    {
        Heightmap16Bit ApplySmooth(Heightmap16Bit source, SmoothStrength level);
    }

    public class HeightmapSmoothingService : IHeightmapSmoothingService
    {
        public Heightmap16Bit ApplySmooth(Heightmap16Bit source, SmoothStrength level)
        {
            if (level == SmoothStrength.None || source == null)
                return source.Clone();

            var smoothed = source.Clone();
            int iterations = (int)level;

            for (int i = 0; i < iterations; i++)
            {
                smoothed = ApplyBoxBlur3x3(smoothed);
            }

            return smoothed;
        }

        /// <summary>
        /// Aplica filtro box blur 3x3
        /// </summary>
        private Heightmap16Bit ApplyBoxBlur3x3(Heightmap16Bit source)
        {
            var blurred = new Heightmap16Bit(source.Width, source.Height);
            int width = source.Width;
            int height = source.Height;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int sum = 0;
                    int count = 0;

                    // Vizinhança 3x3
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            int nx = x + dx;
                            int ny = y + dy;

                            // Clamp to edges
                            if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                            {
                                sum += source.GetHeight(nx, ny);
                                count++;
                            }
                        }
                    }

                    ushort average = (ushort)(sum / count);
                    blurred.SetHeight(x, y, average);
                }
            }

            return blurred;
        }
    }
}
