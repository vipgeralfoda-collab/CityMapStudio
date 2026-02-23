using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using CityMapStudio.Domain.Models;

namespace CityMapStudio.Infrastructure.IO
{
    public interface IHeightmapExporter
    {
        void ExportAsPng(Heightmap16Bit heightmap, string outputPath);
    }

    public class HeightmapExporter : IHeightmapExporter
    {
        public void ExportAsPng(Heightmap16Bit heightmap, string outputPath)
        {
            if (heightmap == null)
                throw new ArgumentNullException(nameof(heightmap));

            if (string.IsNullOrWhiteSpace(outputPath))
                throw new ArgumentException("Output path cannot be empty", nameof(outputPath));

            var image = new Image<L16>(heightmap.Width, heightmap.Height);

            for (int y = 0; y < heightmap.Height; y++)
            {
                for (int x = 0; x < heightmap.Width; x++)
                {
                    ushort heightValue = heightmap.GetHeight(x, y);
                    image[x, y] = new L16(heightValue);
                }
            }

            image.SaveAsPng(outputPath);
        }
    }
}
