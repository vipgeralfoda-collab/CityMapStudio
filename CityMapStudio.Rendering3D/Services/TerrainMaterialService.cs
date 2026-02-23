using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace CityMapStudio.Rendering3D.Services
{
    /// <summary>
    /// Serviço para aplicar materiais realistas ao terreno
    /// </summary>
    public class TerrainMaterialService
    {
        /// <summary>
        /// Cria material para o topo do terreno (grama/solo)
        /// </summary>
        public static Material CreateTerrainTopMaterial()
        {
            // Verde natural com especularidade suave
            var brush = new SolidColorBrush(Color.FromRgb(74, 124, 58)); // #4A7C3A
            return new DiffuseMaterial(brush);
        }

        /// <summary>
        /// Cria material para as laterais do terreno (solo/rocha)
        /// </summary>
        public static Material CreateTerrainSideMaterial()
        {
            // Solo mais escuro nas laterais
            var brush = new SolidColorBrush(Color.FromRgb(58, 106, 42)); // #3A6A2A
            return new DiffuseMaterial(brush);
        }

        /// <summary>
        /// Cria material para água (azul translúcido)
        /// </summary>
        public static Material CreateWaterMaterial()
        {
            // Azul da água
            var brush = new SolidColorBrush(Color.FromRgb(61, 141, 201)); // #3D8DC9
            return new DiffuseMaterial(brush);
        }

        /// <summary>
        /// Cria material com efeito de gradiente baseado em altura
        /// </summary>
        public static Material CreateHeightBasedMaterial(float heightNormalized)
        {
            // Variar de cor baseado na altura (verde claro no topo, escuro na base)
            byte r = (byte)(74 * heightNormalized + 58 * (1 - heightNormalized));
            byte g = (byte)(124 * heightNormalized + 106 * (1 - heightNormalized));
            byte b = (byte)(58 * heightNormalized + 42 * (1 - heightNormalized));

            var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
            return new DiffuseMaterial(brush);
        }

        /// <summary>
        /// Aplica material ao geometry model e ajusta iluminação
        /// </summary>
        public static void ApplyMaterialWithLighting(GeometryModel3D model, Material topMaterial, Material sideMaterial)
        {
            model.Material = topMaterial;
            model.BackMaterial = sideMaterial ?? topMaterial;
        }
    }
}
