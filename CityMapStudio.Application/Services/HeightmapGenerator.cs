using CityMapStudio.Domain.Models;

namespace CityMapStudio.Application.Services
{
    public interface IHeightmapGenerator
    {
        Heightmap16Bit Generate(int width, int height, float scale, int seed = 0);
    }

    public class HeightmapGenerator : IHeightmapGenerator
    {
        public Heightmap16Bit Generate(int width, int height, float scale, int seed = 0)
        {
            var heightmap = new Heightmap16Bit(width, height);
            Random random = new Random(seed);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Padrão procedural simples: seno + coseno para criar ondulações
                    float normalizedX = (float)x / width;
                    float normalizedY = (float)y / height;

                    float value = 0f;
                    
                    // Camadas de seno/coseno com diferentes frequências (similar a Perlin)
                    value += (float)Math.Sin(normalizedX * Math.PI * 2) * 0.5f;
                    value += (float)Math.Cos(normalizedY * Math.PI * 2) * 0.5f;
                    value += (float)Math.Sin(normalizedX * Math.PI * 4) * 0.25f;
                    value += (float)Math.Cos(normalizedY * Math.PI * 4) * 0.25f;

                    // Adiciona um pouco de aleatoriedade
                    value += (float)(random.NextDouble() - 0.5f) * 0.1f;

                    // Normalizar para 0-1
                    value = (value + 1.5f) / 3f;
                    value = Math.Max(0, Math.Min(1, value));

                    // Converter para ushort (0-65535)
                    ushort heightValue = (ushort)(value * scale * ushort.MaxValue);
                    heightmap.SetHeight(x, y, heightValue);
                }
            }

            return heightmap;
        }
    }
}
