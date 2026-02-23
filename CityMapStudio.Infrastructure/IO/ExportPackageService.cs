using CityMapStudio.Domain.Models;

namespace CityMapStudio.Infrastructure.IO
{
    public interface IExportPackageService
    {
        void ExportMapPackage(MapProject project, Heightmap16Bit heightmap, string destinationFolder);
    }

    public class ExportPackageService : IExportPackageService
    {
        private readonly IHeightmapExporter _heightmapExporter;
        private readonly IProjectJsonWriter _jsonWriter;
        private readonly IReadmeGenerator _readmeGenerator;

        public ExportPackageService(
            IHeightmapExporter heightmapExporter,
            IProjectJsonWriter jsonWriter,
            IReadmeGenerator readmeGenerator)
        {
            _heightmapExporter = heightmapExporter ?? throw new ArgumentNullException(nameof(heightmapExporter));
            _jsonWriter = jsonWriter ?? throw new ArgumentNullException(nameof(jsonWriter));
            _readmeGenerator = readmeGenerator ?? throw new ArgumentNullException(nameof(readmeGenerator));
        }

        public void ExportMapPackage(MapProject project, Heightmap16Bit heightmap, string destinationFolder)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project));

            if (heightmap == null)
                throw new ArgumentNullException(nameof(heightmap));

            if (string.IsNullOrWhiteSpace(destinationFolder))
                throw new ArgumentException("Destination folder cannot be empty", nameof(destinationFolder));

            // Validar projeto
            var (isValid, errorMessage) = project.Validate();
            if (!isValid)
                throw new InvalidOperationException(errorMessage);

            // Criar pasta do pacote
            string packageFolder = Path.Combine(destinationFolder, SanitizeFolderName(project.MapName));
            
            if (!Directory.Exists(destinationFolder))
                throw new DirectoryNotFoundException($"Destination folder not found: {destinationFolder}");

            try
            {
                Directory.CreateDirectory(packageFolder);

                // Exportar heightmap
                string heightmapPath = Path.Combine(packageFolder, "heightmap.png");
                _heightmapExporter.ExportAsPng(heightmap, heightmapPath);

                // Exportar project.json
                string jsonPath = Path.Combine(packageFolder, "project.json");
                _jsonWriter.WriteProjectMetadata(project, jsonPath);

                // Gerar README
                string readmePath = Path.Combine(packageFolder, "README.txt");
                _readmeGenerator.GenerateReadme(readmePath, project.MapName);
            }
            catch (Exception ex)
            {
                // Limpar pasta se algo der errado
                try
                {
                    if (Directory.Exists(packageFolder))
                        Directory.Delete(packageFolder, true);
                }
                catch { /* Ignorar erros ao limpar */ }

                throw new InvalidOperationException($"Erro ao exportar pacote: {ex.Message}", ex);
            }
        }

        private string SanitizeFolderName(string name)
        {
            // Remover caracteres inválidos para pasta
            char[] invalidChars = Path.GetInvalidPathChars().Concat(new[] { '<', '>', ':', '"', '/', '\\', '|', '?', '*' }).ToArray();
            string sanitized = new string(name.Where(c => !invalidChars.Contains(c)).ToArray());
            
            // Se ficar vazio, usar padrão
            if (string.IsNullOrWhiteSpace(sanitized))
                sanitized = "Map_" + DateTime.UtcNow.Ticks;

            return sanitized.Trim();
        }
    }
}
