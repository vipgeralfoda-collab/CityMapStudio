namespace CityMapStudio.Domain.Models
{
    /// <summary>
    /// Metadados do projeto de mapa para exportação
    /// </summary>
    public class MapProject
    {
        public string MapName { get; set; } = "Untitled Map";
        public int Width { get; set; }
        public int Height { get; set; }
        public int BitDepth { get; set; } = 16;
        public float VerticalScaleMeters { get; set; } = 50f;
        public float SeaLevelMeters { get; set; } = 0f;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public MapProject() { }

        public MapProject(string mapName, Heightmap16Bit heightmap, float verticalScale, float seaLevel)
        {
            MapName = mapName;
            Width = heightmap.Width;
            Height = heightmap.Height;
            BitDepth = 16;
            VerticalScaleMeters = verticalScale;
            SeaLevelMeters = seaLevel;
            CreatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Valida se o mapa é compatível com Cities: Skylines II
        /// </summary>
        public (bool isValid, string errorMessage) Validate()
        {
            if (Width != 4096 || Height != 4096)
            {
                return (false, $"Dimensão incompatível: {Width}x{Height}. Cities: Skylines II requer 4096x4096.");
            }

            if (BitDepth != 16)
            {
                return (false, $"Profundidade de cor incompatível: {BitDepth}-bit. Requerido: 16-bit.");
            }

            if (string.IsNullOrWhiteSpace(MapName))
            {
                return (false, "Nome do mapa não pode estar vazio.");
            }

            return (true, "");
        }
    }
}
