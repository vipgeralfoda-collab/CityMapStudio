namespace CityMapStudio.Domain.Models
{
    /// <summary>
    /// Configurações globais do terreno (Sea Level, Vertical Scale, etc)
    /// </summary>
    public class TerrainSettings
    {
        public float VerticalScaleMeters { get; set; } = 50f;
        public float SeaLevelMeters { get; set; } = 0f;
        public SmoothStrength SmoothLevel { get; set; } = SmoothStrength.None;
        public bool ApplySmoothOnExport { get; set; } = false;
        public float TargetMaxHeightMeters { get; set; } = 300f;
        public bool NormalizeForCS2 { get; set; } = false;

        /// <summary>
        /// Retorna min/max height em metros baseado no heightmap e vertical scale
        /// </summary>
        public static (float min, float max) GetHeightRange(Heightmap16Bit heightmap, float verticalScale)
        {
            if (heightmap == null || heightmap.Data.Length == 0)
                return (0f, verticalScale);

            ushort minValue = ushort.MaxValue;
            ushort maxValue = ushort.MinValue;

            foreach (var height in heightmap.Data)
            {
                if (height < minValue) minValue = height;
                if (height > maxValue) maxValue = height;
            }

            float minHeight = (minValue / 65535f) * verticalScale;
            float maxHeight = (maxValue / 65535f) * verticalScale;

            return (minHeight, maxHeight);
        }

        /// <summary>
        /// Clamp Sea Level dentro do range do heightmap
        /// </summary>
        public static float ClampSeaLevel(float seaLevel, Heightmap16Bit heightmap, float verticalScale)
        {
            var (minHeight, maxHeight) = GetHeightRange(heightmap, verticalScale);
            return Math.Max(minHeight, Math.Min(maxHeight, seaLevel));
        }
    }
}
