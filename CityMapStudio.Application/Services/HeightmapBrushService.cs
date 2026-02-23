using CityMapStudio.Domain.Models;

namespace CityMapStudio.Application.Services
{
    public interface IHeightmapBrushService
    {
        /// <summary>
        /// Aplica um stroke de brush no heightmap
        /// Retorna a dirty region que foi alterada
        /// </summary>
        RectI ApplyBrush(
            Heightmap16Bit heightmap,
            int centerX,
            int centerY,
            TerrainBrushMode mode,
            BrushSettings settings);
    }

    public class HeightmapBrushService : IHeightmapBrushService
    {
        public RectI ApplyBrush(
            Heightmap16Bit heightmap,
            int centerX,
            int centerY,
            TerrainBrushMode mode,
            BrushSettings settings)
        {
            int width = heightmap.Width;
            int height = heightmap.Height;

            // Calcular área afetada
            int minX = Math.Max(0, centerX - settings.Radius);
            int minY = Math.Max(0, centerY - settings.Radius);
            int maxX = Math.Min(width - 1, centerX + settings.Radius);
            int maxY = Math.Min(height - 1, centerY + settings.Radius);

            var dirtyRegion = new RectI(minX, minY, maxX - minX + 1, maxY - minY + 1);

            // Aplicar brush conforme o modo
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    float distX = x - centerX;
                    float distY = y - centerY;
                    float distance = (float)Math.Sqrt(distX * distX + distY * distY);

                    if (distance > settings.Radius)
                        continue;

                    float falloff = CalculateFalloff(distance, settings.Radius, settings.Falloff);
                    float amount = settings.Strength * falloff;

                    ushort currentHeight = heightmap.GetHeight(x, y);

                    ushort newHeight = mode switch
                    {
                        TerrainBrushMode.Raise => (ushort)Math.Min(65535, currentHeight + (int)(amount * 500)),
                        TerrainBrushMode.Lower => (ushort)Math.Max(0, currentHeight - (int)(amount * 500)),
                        TerrainBrushMode.Flatten => LerpHeight(currentHeight, (ushort)(settings.TargetHeight * 65535), amount),
                        TerrainBrushMode.SetHeight => (ushort)(settings.TargetHeight * 65535),
                        TerrainBrushMode.Smooth => SmoothHeight(heightmap, x, y, amount),
                        TerrainBrushMode.Noise => AddNoise(currentHeight, amount),
                        _ => currentHeight
                    };

                    heightmap.SetHeight(x, y, newHeight);
                }
            }

            return dirtyRegion;
        }

        private float CalculateFalloff(float distance, int radius, BrushFalloff falloff)
        {
            float normalized = distance / radius;
            normalized = Math.Max(0, 1 - normalized);

            return falloff switch
            {
                BrushFalloff.Linear => normalized,
                BrushFalloff.Smooth => normalized * normalized * (3 - 2 * normalized), // Smoothstep
                BrushFalloff.Sharp => normalized > 0.5f ? 1 : 0,
                _ => normalized
            };
        }

        private ushort LerpHeight(ushort from, ushort to, float t)
        {
            return (ushort)(from * (1 - t) + to * t);
        }

        private ushort SmoothHeight(Heightmap16Bit heightmap, int x, int y, float strength)
        {
            int sum = 0;
            int count = 0;

            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    int nx = x + dx;
                    int ny = y + dy;

                    if (nx >= 0 && nx < heightmap.Width && ny >= 0 && ny < heightmap.Height)
                    {
                        sum += heightmap.GetHeight(nx, ny);
                        count++;
                    }
                }
            }

            ushort average = (ushort)(sum / count);
            ushort current = heightmap.GetHeight(x, y);

            return LerpHeight(current, average, strength);
        }

        private ushort AddNoise(ushort current, float strength)
        {
            int noise = (int)((Math.Sin(DateTime.Now.Ticks * 0.0001) * 0.5 + 0.5) * 1000 * strength);
            return (ushort)Math.Clamp(current + noise, 0, 65535);
        }
    }
}
