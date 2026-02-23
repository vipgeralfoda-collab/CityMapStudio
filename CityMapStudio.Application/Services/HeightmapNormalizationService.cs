using CityMapStudio.Domain.Models;

namespace CityMapStudio.Application.Services
{
    public interface IHeightmapNormalizationService
    {
        Heightmap16Bit NormalizeForCS2(Heightmap16Bit source, float targetMaxHeightMeters, float verticalScale);
    }

    public class HeightmapNormalizationService : IHeightmapNormalizationService
    {
        /// <summary>
        /// Normaliza heightmap para melhor distribuição de cores em CS2
        /// Remapeia os valores para utilizar melhor a gama disponível
        /// </summary>
        public Heightmap16Bit NormalizeForCS2(Heightmap16Bit source, float targetMaxHeightMeters, float verticalScale)
        {
            if (source == null)
                return source;

            var normalized = new Heightmap16Bit(source.Width, source.Height);
            var stats = source.GetStatistics();

            // Calcular fator de normalização
            // targetMaxHeightMeters define a altura máxima desejada
            // Isso se traduz em valor ushort
            ushort targetMax = (ushort)(Math.Min(targetMaxHeightMeters / verticalScale * 65535f, 65535f));

            // Se o range atual já é bom, não normalize muito
            if (stats.MaxHeight == 0)
                return source.Clone();

            float scaleFactor = targetMax / (float)stats.MaxHeight;

            for (int i = 0; i < source.Data.Length; i++)
            {
                ushort originalValue = source.Data[i];
                
                // Normalizar mantendo proporção
                float normalized_value = originalValue * scaleFactor;
                normalized.Data[i] = (ushort)Math.Min(normalized_value, 65535);
            }

            return normalized;
        }
    }
}
