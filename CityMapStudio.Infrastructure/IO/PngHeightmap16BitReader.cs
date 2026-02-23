using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using CityMapStudio.Domain.Models;

namespace CityMapStudio.Infrastructure.IO
{
    public interface IPngHeightmap16BitReader
    {
        Heightmap16Bit Read(string filePath);
    }

    public class PngHeightmap16BitReader : IPngHeightmap16BitReader
    {
        public Heightmap16Bit Read(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Arquivo não encontrado: {filePath}");

            if (!filePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("O arquivo deve ser PNG (.png)");

            try
            {
                using (var image = Image.Load(filePath))
                {
                    // Verificar se é 16-bit grayscale
                    if (image.Metadata.DecodedImageFormat?.DefaultMimeType != "image/png")
                        throw new InvalidOperationException("Arquivo não é um PNG válido");

                    // Tentar converter para L16 (grayscale 16-bit)
                    if (image is not Image<L16> image16Bit)
                    {
                        // Se for outro formato, verificar se é conversível
                        if (image is Image<Rgba32> imageRgba)
                        {
                            // Converter RGBA para L16
                            var converted = image.CloneAs<L16>();
                            return ReadFromImage(converted);
                        }
                        else if (image is Image<L8> image8Bit)
                        {
                            throw new InvalidOperationException(
                                "Arquivo é PNG 8-bit (grayscale). Use PNG 16-bit (grayscale).");
                        }
                        else
                        {
                            throw new InvalidOperationException(
                                "Formato PNG não suportado. Use grayscale 16-bit.");
                        }
                    }

                    return ReadFromImage(image16Bit);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Erro ao carregar PNG: {ex.Message}", ex);
            }
        }

        private Heightmap16Bit ReadFromImage(Image<L16> image)
        {
            int width = image.Width;
            int height = image.Height;

            // Validar dimensões mínimas
            if (width < 2 || height < 2)
                throw new InvalidOperationException("Dimensões mínimas do heightmap: 2x2 pixels");

            // Validar dimensões máximas (evitar memory issues)
            if (width > 4096 || height > 4096)
                throw new InvalidOperationException(
                    $"Dimensões muito grandes: {width}x{height}. Máximo: 4096x4096");

            var heightmap = new Heightmap16Bit(width, height);

            // Ler pixels diretamente
            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < height; y++)
                {
                    Span<L16> pixelRow = accessor.GetRowSpan(y);
                    for (int x = 0; x < width; x++)
                    {
                        ushort heightValue = pixelRow[x].PackedValue;
                        heightmap.SetHeight(x, y, heightValue);
                    }
                }
            });

            return heightmap;
        }
    }
}
