namespace CityMapStudio.Domain.Models
{
    /// <summary>
    /// Categorias principais do editor
    /// </summary>
    public enum EditorCategory
    {
        Terrain,
        Water,
        Resources,
        Utilities
    }

    /// <summary>
    /// Modos de edição de terreno
    /// </summary>
    public enum TerrainBrushMode
    {
        Raise,
        Lower,
        Smooth,
        Flatten,
        SetHeight,
        Noise
    }

    /// <summary>
    /// Tipo de falloff do brush
    /// </summary>
    public enum BrushFalloff
    {
        Linear,
        Smooth,
        Sharp
    }

    /// <summary>
    /// Configurações gerais do brush
    /// </summary>
    public class BrushSettings
    {
        public int Radius { get; set; } = 64;
        public float Strength { get; set; } = 1.0f;
        public BrushFalloff Falloff { get; set; } = BrushFalloff.Smooth;
        public int Spacing { get; set; } = 1;
        public float TargetHeight { get; set; } = 0f;

        public BrushSettings Clone()
        {
            return new BrushSettings
            {
                Radius = Radius,
                Strength = Strength,
                Falloff = Falloff,
                Spacing = Spacing,
                TargetHeight = TargetHeight
            };
        }
    }

    /// <summary>
    /// Estado de edição do mapa
    /// </summary>
    public class EditorState
    {
        public EditorCategory CurrentCategory { get; set; } = EditorCategory.Terrain;
        public TerrainBrushMode CurrentTerrainMode { get; set; } = TerrainBrushMode.Raise;
        public BrushSettings BrushSettings { get; set; } = new BrushSettings();
        public bool IsEditing { get; set; } = false;
    }
}
