using CityMapStudio.Domain.Models;

namespace CityMapStudio.Infrastructure.IO
{
    public interface IImportHeightmapFromPng
    {
        Heightmap16Bit ImportFromFile(string filePath);
    }

    public class ImportHeightmapFromPng : IImportHeightmapFromPng
    {
        private readonly IPngHeightmap16BitReader _reader;

        public ImportHeightmapFromPng(IPngHeightmap16BitReader reader)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        }

        public Heightmap16Bit ImportFromFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Caminho do arquivo não pode estar vazio", nameof(filePath));

            try
            {
                var heightmap = _reader.Read(filePath);

                // Validar dimensões mínimas
                if (heightmap.Width < 2 || heightmap.Height < 2)
                    throw new InvalidOperationException("Heightmap deve ter pelo menos 2x2 pixels");

                return heightmap;
            }
            catch (FileNotFoundException ex)
            {
                throw new InvalidOperationException($"Arquivo não encontrado: {filePath}", ex);
            }
            catch (InvalidOperationException ex)
            {
                // Re-lançar com contexto de aplicação
                throw new InvalidOperationException($"Erro ao importar heightmap: {ex.Message}", ex);
            }
        }
    }
}
