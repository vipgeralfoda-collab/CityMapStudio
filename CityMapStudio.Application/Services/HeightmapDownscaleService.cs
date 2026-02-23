using CityMapStudio.Domain.Models;

namespace CityMapStudio.Application.Services
{
    public interface IHeightmapDownscaleService
    {
        Heightmap16Bit CreatePreview(Heightmap16Bit full, PreviewResolution resolution, RectI? dirtyRegion = null);
    }

    public class HeightmapDownscaleService : IHeightmapDownscaleService
    {
        /// <summary>
        /// Cria um preview downscalado do heightmap full (4096)
        /// Se dirtyRegion for null, gera preview completo
        /// Se dirtyRegion for informada, atualiza apenas aquela região (para performance)
        /// </summary>
        public Heightmap16Bit CreatePreview(Heightmap16Bit full, PreviewResolution resolution, RectI? dirtyRegion = null)
        {
            if (full == null)
                return null;

            int previewSize = (int)resolution;
            float scale = previewSize / 4096f;

            var preview = new Heightmap16Bit(previewSize, previewSize);

            if (dirtyRegion == null)
            {
                // Gerar preview completo
                GeneratePreviewFull(full, preview, scale);
            }
            else
            {
                // Atualizar apenas a região dirty
                UpdatePreviewPartial(full, preview, scale, dirtyRegion.Value);
            }

            return preview;
        }

        /// <summary>
        /// Gera preview completo downscalando com média por bloco
        /// </summary>
        private void GeneratePreviewFull(Heightmap16Bit full, Heightmap16Bit preview, float scale)
        {
            int fullWidth = full.Width;
            int fullHeight = full.Height;
            int previewWidth = preview.Width;
            int previewHeight = preview.Height;

            float blockSize = 1f / scale; // Tamanho do bloco no full-res que corresponde a 1 pixel do preview

            for (int py = 0; py < previewHeight; py++)
            {
                for (int px = 0; px < previewWidth; px++)
                {
                    // Coordenadas no full-res
                    float fullX = px * blockSize;
                    float fullY = py * blockSize;

                    // Amostra uma região 2x2 e faz média (anti-aliasing)
                    int x0 = (int)fullX;
                    int y0 = (int)fullY;
                    int x1 = Math.Min(x0 + (int)blockSize, fullWidth - 1);
                    int y1 = Math.Min(y0 + (int)blockSize, fullHeight - 1);

                    double sum = 0;
                    int count = 0;

                    for (int fy = y0; fy <= y1; fy++)
                    {
                        for (int fx = x0; fx <= x1; fx++)
                        {
                            sum += full.GetHeight(fx, fy);
                            count++;
                        }
                    }

                    ushort average = (ushort)(sum / count);
                    preview.SetHeight(px, py, average);
                }
            }
        }

        /// <summary>
        /// Atualiza apenas a região dirty do preview
        /// </summary>
        private void UpdatePreviewPartial(Heightmap16Bit full, Heightmap16Bit preview, float scale, RectI dirtyRegion)
        {
            int fullWidth = full.Width;
            int fullHeight = full.Height;
            int previewWidth = preview.Width;
            int previewHeight = preview.Height;

            float blockSize = 1f / scale;

            // Converter dirty region do full-res para coordenadas de preview
            int previewStartX = Math.Max(0, (int)(dirtyRegion.X * scale));
            int previewStartY = Math.Max(0, (int)(dirtyRegion.Y * scale));
            int previewEndX = Math.Min(previewWidth - 1, (int)((dirtyRegion.X + dirtyRegion.Width) * scale) + 1);
            int previewEndY = Math.Min(previewHeight - 1, (int)((dirtyRegion.Y + dirtyRegion.Height) * scale) + 1);

            for (int py = previewStartY; py <= previewEndY; py++)
            {
                for (int px = previewStartX; px <= previewEndX; px++)
                {
                    float fullX = px * blockSize;
                    float fullY = py * blockSize;

                    int x0 = (int)fullX;
                    int y0 = (int)fullY;
                    int x1 = Math.Min(x0 + (int)blockSize, fullWidth - 1);
                    int y1 = Math.Min(y0 + (int)blockSize, fullHeight - 1);

                    double sum = 0;
                    int count = 0;

                    for (int fy = y0; fy <= y1; fy++)
                    {
                        for (int fx = x0; fx <= x1; fx++)
                        {
                            sum += full.GetHeight(fx, fy);
                            count++;
                        }
                    }

                    ushort average = (ushort)(sum / count);
                    preview.SetHeight(px, py, average);
                }
            }
        }
    }
}
